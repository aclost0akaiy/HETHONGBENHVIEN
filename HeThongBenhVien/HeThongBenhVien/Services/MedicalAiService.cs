using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using HeThongBenhVien.Models;


namespace HeThongBenhVien.Services
{
    public class AiRequestLogEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString().Substring(0, 8);
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Module { get; set; } = string.Empty; // "ICD-10", "Vision", "Department", "Chat"
        public string Input { get; set; } = string.Empty;
        public string RawResponse { get; set; } = string.Empty;
        public bool Success { get; set; }
        public double ExecutionTimeMs { get; set; }
        public string? ValidationError { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class IcdDiagnosticResult
    {
        public string IcdCode { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string TreatmentPlan { get; set; } = string.Empty;
        public string LabTests { get; set; } = string.Empty;
        public string Medicines { get; set; } = string.Empty;
        public string Disclaimer { get; set; } = string.Empty;
    }

    public class VisionAnalysisResult
    {
        public string Finding { get; set; } = "Bình thường";
        public double Confidence { get; set; } = 100.0;
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public string Quality { get; set; } = "Đạt tiêu chuẩn";
        public string Lesion { get; set; } = "Không phát hiện";
        public string Alignment { get; set; } = "Bình thường";
        public string SoftTissue { get; set; } = "Bình thường";
        public string Disclaimer { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class DepartmentDto
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
    }

    public class ChatMessageDto
    {
        public string Role { get; set; } = string.Empty; // "user" or "model"
        public string Content { get; set; } = string.Empty;
    }

    public class TestScenarioResult
    {
        public string CaseName { get; set; } = string.Empty;
        public string InputSymptoms { get; set; } = string.Empty;
        public string ExpectedIcdGroup { get; set; } = string.Empty; // e.g. "J" for respiratory, "I" for cardiovascular
        public string PredictedIcd { get; set; } = string.Empty;
        public string PredictedDiagnosis { get; set; } = string.Empty;
        public bool Passed { get; set; }
        public double LatencyMs { get; set; }
        public string ErrorDetails { get; set; } = string.Empty;
    }

    public class MedicalAiService
    {
        private readonly IConfiguration _config;
        private static readonly ConcurrentQueue<AiRequestLogEntry> LogsQueue = new ConcurrentQueue<AiRequestLogEntry>();
        private const int MaxLogs = 100;

        public MedicalAiService(IConfiguration config)
        {
            _config = config;
        }

        private string GetGeminiApiKey(string? customKey = null)
        {
            if (!string.IsNullOrEmpty(customKey)) return customKey.Trim();
            string? key = _config["GeminiApiKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException("API Key Gemini chưa được thiết lập. Vui lòng cấu hình 'GeminiApiKey' trong file appsettings.json.");
            }
            return key.Trim();
        }

        private string? GetOpenAiApiKey(string? customKey = null)
        {
            if (!string.IsNullOrEmpty(customKey)) return customKey.Trim();
            return _config["OpenAiApiKey"]?.Trim();
        }

        public List<AiRequestLogEntry> GetLogs()
        {
            return LogsQueue.OrderByDescending(l => l.Timestamp).ToList();
        }

        public void ClearLogs()
        {
            while (LogsQueue.TryDequeue(out _)) { }
        }

        private void AddLog(AiRequestLogEntry entry)
        {
            LogsQueue.Enqueue(entry);
            while (LogsQueue.Count > MaxLogs)
            {
                LogsQueue.TryDequeue(out _);
            }
        }

        // Retry helper on transient errors or HTTP 429
        private async Task<HttpResponseMessage> SendWithRetryAsync(HttpClient client, HttpRequestMessage request, int maxRetries = 3)
        {
            int retryCount = 0;
            int delayMs = 1000;
            HttpResponseMessage? response = null;

            while (true)
            {
                try
                {
                    // Clone the request for retries since a request can only be sent once
                    HttpRequestMessage reqClone = await CloneRequestAsync(request);
                    response = await client.SendAsync(reqClone);

                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }

                    // Retry on 429 (Rate Limit) or 5xx (Server Errors)
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || 
                        ((int)response.StatusCode >= 500 && (int)response.StatusCode < 600))
                    {
                        if (retryCount >= maxRetries) return response;
                        retryCount++;
                        await Task.Delay(delayMs);
                        delayMs *= 2; // Exponential backoff
                        continue;
                    }

                    return response;
                }
                catch (Exception)
                {
                    if (retryCount >= maxRetries) throw;
                    retryCount++;
                    await Task.Delay(delayMs);
                    delayMs *= 2;
                }
            }
        }

        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);
            clone.Version = req.Version;
            foreach (var prop in req.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object?>(prop.Key), prop.Value);
            }
            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            if (req.Content != null)
            {
                var ms = new MemoryStream();
                await req.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);
                foreach (var header in req.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
            return clone;
        }

        // Robust JSON extractor and cleaner
        private string CleanAndExtractJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return "{}";

            text = text.Trim();
            
            // Regex to extract object between first { and last }
            var match = Regex.Match(text, @"\{.*\}", RegexOptions.Singleline);
            if (match.Success)
            {
                string json = match.Value.Trim();
                
                // Remove potential invalid trailing commas before closing braces/brackets
                json = Regex.Replace(json, @",\s*([\}\]])", "$1");
                return json;
            }

            return "{}";
        }

        // --- MODULE 1: ICD-10 DIAGNOSTIC PREDICTOR ---
        public async Task<IcdDiagnosticResult> PredictIcdFromSymptomsAsync(string symptoms, List<ICD10Protocol> databaseProtocols, string? customApiKey = null)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var log = new AiRequestLogEntry
            {
                Module = "ICD-10",
                Input = $"Symptoms: {symptoms}"
            };

            var finalResult = new IcdDiagnosticResult
            {
                Disclaimer = "Khuyến cáo an toàn: Đây là chẩn đoán gợi ý của AI. Bác sĩ cần khám thực tế và phê duyệt trước khi lưu hồ sơ."
            };

            try
            {
                string apiKey = GetGeminiApiKey(customApiKey);
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={apiKey}";

                var optionsText = string.Join("\n", databaseProtocols.Select(p => $"- {p.ICDCode}: {p.Diagnosis}"));
                
                string promptText = $"Dựa trên các triệu chứng lâm sàng của bệnh nhân sau: \"{symptoms}\"\n\n" +
                    "Hãy thực hiện phân tích chẩn đoán y khoa như một bác sĩ chuyên khoa cấp cao. Hãy chọn mã bệnh ICD-10 phù hợp nhất.\n" +
                    "Nếu triệu chứng trùng khớp hoặc gần giống với một trong các phác đồ hiện có sau đây, hãy dùng đúng mã hiện có:\n" +
                    optionsText + "\n\n" +
                    "Nếu triệu chứng KHÔNG TRÙNG KHỚP với bất kỳ mã nào ở trên, bạn hãy tự suy luận ra một mã ICD-10 thực tế chuẩn quốc tế mới cùng chẩn đoán, cận lâm sàng đề xuất (ngắn gọn) và đơn thuốc (gồm tên thuốc viết hoa chữ cái đầu, ngăn cách nhau bằng dấu phẩy) phù hợp.\n\n" +
                    "Yêu cầu bắt buộc trả về kết quả dưới dạng JSON đúng cấu trúc sau (không kèm markdown): \n" +
                    "{\n" +
                    "  \"icdCode\": \"MÃ_ICD_10\",\n" +
                    "  \"diagnosis\": \"Tên chẩn đoán\",\n" +
                    "  \"treatmentPlan\": \"Phác đồ điều trị đề xuất ngắn gọn\",\n" +
                    "  \"labTests\": \"Xét nghiệm 1, Xét nghiệm 2 (ngăn cách bằng dấu phẩy, tối đa 2-3 dịch vụ)\",\n" +
                    "  \"medicines\": \"Thuốc A, Thuốc B (ngăn cách bằng dấu phẩy, tối đa 2-3 thuốc)\"\n" +
                    "}";

                var payload = new
                {
                    systemInstruction = new
                    {
                        parts = new object[]
                        {
                            new { text = "Bạn là chuyên gia cố vấn y tế, hỗ trợ phân tích triệu chứng để phân loại mã ICD-10 chính xác dựa trên danh sách cho sẵn hoặc tự tạo mã mới phù hợp chuẩn y khoa. Không bao giờ khuyên bệnh nhân tự dùng thuốc độc hại mà không có chỉ định." }
                        }
                    },
                    contents = new object[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = promptText }
                            }
                        }
                    },
                    generationConfig = new { response_mime_type = "application/json" }
                };

                using var client = new HttpClient();
                var reqContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = reqContent };

                var response = await SendWithRetryAsync(client, request);
                log.Success = response.IsSuccessStatusCode;
                
                string responseString = await response.Content.ReadAsStringAsync();
                log.RawResponse = responseString;

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = $"Lỗi từ Google AI API (HTTP {(int)response.StatusCode}): {responseString}";
                    log.ErrorMessage = errorMsg;
                    throw new HttpRequestException(errorMsg);
                }

                using var doc = JsonDocument.Parse(responseString);
                string? rawText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrEmpty(rawText))
                {
                    throw new InvalidOperationException("AI không trả về nội dung văn bản chẩn đoán.");
                }

                string cleanJson = CleanAndExtractJson(rawText);
                using var resDoc = JsonDocument.Parse(cleanJson);
                var root = resDoc.RootElement;

                // Parse properties safely with case-insensitivity fallback
                string icdCode = root.TryGetProperty("icdCode", out var cProp) ? cProp.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(icdCode) && root.TryGetProperty("IcdCode", out var cPropCap)) icdCode = cPropCap.GetString() ?? "";

                string diagnosis = root.TryGetProperty("diagnosis", out var dProp) ? dProp.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(diagnosis) && root.TryGetProperty("Diagnosis", out var dPropCap)) diagnosis = dPropCap.GetString() ?? "";

                string treatment = root.TryGetProperty("treatmentPlan", out var tProp) ? tProp.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(treatment) && root.TryGetProperty("TreatmentPlan", out var tPropCap)) treatment = tPropCap.GetString() ?? "";

                string labs = root.TryGetProperty("labTests", out var lProp) ? lProp.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(labs) && root.TryGetProperty("LabTests", out var lPropCap)) labs = lPropCap.GetString() ?? "";

                string meds = root.TryGetProperty("medicines", out var mProp) ? mProp.GetString() ?? "" : "";
                if (string.IsNullOrEmpty(meds) && root.TryGetProperty("Medicines", out var mPropCap)) meds = mPropCap.GetString() ?? "";

                // Format validation: ICD-10 code should start with a letter and have digits
                icdCode = icdCode.Trim().ToUpper();
                if (!Regex.IsMatch(icdCode, @"^[A-Z][0-9]"))
                {
                    log.ValidationError = $"Mã ICD-10 không hợp lệ: '{icdCode}'. Hệ thống tự động sửa đổi hoặc đặt mã mặc định.";
                    var codeMatch = Regex.Match(icdCode, @"[A-Z][0-9]{2}(\.[0-9])?");
                    icdCode = codeMatch.Success ? codeMatch.Value : "R50"; // R50: Sốt không rõ nguyên nhân
                }

                return new IcdDiagnosticResult
                {
                    IcdCode = icdCode,
                    Diagnosis = string.IsNullOrEmpty(diagnosis) ? "Chẩn đoán lâm sàng đề xuất" : diagnosis,
                    TreatmentPlan = string.IsNullOrEmpty(treatment) ? "Theo dõi sức khỏe và tái khám khi có bất thường." : treatment,
                    LabTests = labs,
                    Medicines = meds,
                    Disclaimer = "Khuyến cáo an toàn: Đây là chẩn đoán gợi ý của AI. Bác sĩ cần khám thực tế và phê duyệt trước khi lưu hồ sơ."
                };
            }
            catch (Exception ex)
            {
                log.Success = false;
                log.ErrorMessage = ex.Message;
                throw;
            }
            finally
            {
                watch.Stop();
                log.ExecutionTimeMs = watch.Elapsed.TotalMilliseconds;
                AddLog(log);
            }

            return finalResult;
        }

        // --- MODULE 2: CLINICAL IMAGE ANALYSIS (VISION) ---
        public async Task<VisionAnalysisResult> AnalyzeImageAiAsync(byte[] imageBytes, string contentType, string? customApiKey = null)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var log = new AiRequestLogEntry
            {
                Module = "Vision",
                Input = $"Image Analysis: {contentType}, Size: {imageBytes.Length} bytes"
            };

            var finalResult = new VisionAnalysisResult
            {
                Disclaimer = "Cảnh báo: Kết quả phân tích hình ảnh của AI mang tính chất hỗ trợ và cần được bác sĩ chuyên khoa chẩn đoán hình ảnh ký duyệt lâm sàng."
            };

            try
            {
                string apiKey = GetGeminiApiKey(customApiKey);
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={apiKey}";
                string base64Image = Convert.ToBase64String(imageBytes);

                string promptText = "Bạn là một bác sĩ chẩn đoán hình ảnh cấp cao. Hãy thực hiện kiểm tra y khoa hình ảnh cận lâm sàng này.\n" +
                    "YÊU CẦU QUAN TRỌNG VỀ ĐỘ TIN CẬY:\n" +
                    "1. Trước tiên, hãy phân tích xem hình ảnh này có thực sự là ảnh cận lâm sàng y tế (X-quang, CT, MRI, Siêu âm, X-quang nha khoa, v.v.) hay không.\n" +
                    "   - Nếu KHÔNG PHẢI là ảnh y tế, hãy lập tức trả về: {\"finding\": \"Hình ảnh không phải là cận lâm sàng y tế\", \"confidence\": 0.0, \"x\": 0, \"y\": 0, \"width\": 0, \"height\": 0, \"quality\": \"Không hợp lệ\", \"lesion\": \"Không thể xác định\", \"alignment\": \"Không thể xác định\", \"softTissue\": \"Không thể xác định\"}.\n" +
                    "2. Nếu ĐÚNG là ảnh cận lâm sàng y tế, hãy phân tích và chỉ ra các tổn thương hoặc bất bất thường nếu có. Phác họa vùng tổn thương bằng tọa độ chuẩn hóa x, y, width, height (từ 0 đến 100 đại diện cho phần trăm chiều rộng/chiều cao ảnh).\n" +
                    "3. Trường 'finding' phải cực kỳ NGẮN GỌN (dưới 15 từ). Ví dụ: 'Nứt đầu dưới xương mác cẳng chân' hoặc 'Răng khôn 38 mọc lệch 90 độ' hoặc 'Bình thường'.\n" +
                    "4. Đánh giá chi tiết các tiêu chí:\n" +
                    "   - quality: Chất lượng hình ảnh (ví dụ: 'Đạt tiêu chuẩn', 'Mờ nhẹ').\n" +
                    "   - lesion: Nhận diện tổn thương (ví dụ: 'Có vết gãy/nứt', 'Mọc lệch/ngầm', 'Không phát hiện').\n" +
                    "   - alignment: Trạng thái khớp/vị trí giải phẫu (ví dụ: 'Bình thường', 'Di lệch nhẹ').\n" +
                    "   - softTissue: Tình trạng mô mềm xung quanh (ví dụ: 'Sưng nề nhẹ', 'Bình thường').\n" +
                    "Trả về kết quả DƯỚI DẠNG JSON đúng cấu trúc sau (không kèm markdown): {\"finding\": \"Mô tả ngắn gọn\", \"confidence\": 95.0, \"x\": 10, \"y\": 20, \"width\": 5, \"height\": 5, \"quality\": \"Đạt tiêu chuẩn\", \"lesion\": \"Không phát hiện\", \"alignment\": \"Bình thường\", \"softTissue\": \"Bình thường\"}.";

                var payload = new
                {
                    systemInstruction = new
                    {
                        parts = new object[]
                        {
                            new { text = "Bạn là chuyên gia chẩn đoán hình ảnh hàng đầu. Luôn trung thực và khách quan. Phát hiện ảnh không hợp lệ để tránh sai sót y khoa." }
                        }
                    },
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = promptText },
                                new { inline_data = new { mime_type = contentType, data = base64Image } }
                            }
                        }
                    },
                    generationConfig = new { response_mime_type = "application/json" }
                };

                using var client = new HttpClient();
                var reqContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = reqContent };

                var response = await SendWithRetryAsync(client, request);
                log.Success = response.IsSuccessStatusCode;

                string responseString = await response.Content.ReadAsStringAsync();
                log.RawResponse = responseString;

                if (!response.IsSuccessStatusCode)
                {
                    string errorMsg = $"Lỗi kết nối Google AI Vision API (HTTP {(int)response.StatusCode}): {responseString}";
                    log.ErrorMessage = errorMsg;
                    throw new HttpRequestException(errorMsg);
                }

                using var doc = JsonDocument.Parse(responseString);
                string? rawText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrEmpty(rawText))
                {
                    throw new InvalidOperationException("AI Vision không trả về nội dung chẩn đoán hình ảnh.");
                }

                string cleanJson = CleanAndExtractJson(rawText);
                try
                {
                    System.IO.File.WriteAllText("c:\\Users\\PC\\Downloads\\HETHONGBENHVIEN\\HeThongBenhVien\\HeThongBenhVien\\wwwroot\\uploads\\raw_response.txt", "--- RAW ---\n" + rawText + "\n--- CLEAN ---\n" + cleanJson);
                }
                catch {}
                using var resDoc = JsonDocument.Parse(cleanJson);
                var root = resDoc.RootElement;

                double GetDoubleSafe(JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.Number) return element.GetDouble();
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        string s = element.GetString() ?? "";
                        s = s.Replace("%", "").Trim();
                        if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                            return val;
                    }
                    return 90.0;
                }

                int GetIntSafe(JsonElement element)
                {
                    if (element.ValueKind == JsonValueKind.Number) return (int)element.GetDouble();
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        string s = element.GetString() ?? "";
                        s = s.Replace("%", "").Trim();
                        if (double.TryParse(s, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double val))
                            return (int)val;
                    }
                    return 0;
                }

                if (root.TryGetProperty("finding", out var fProp)) finalResult.Finding = fProp.GetString() ?? "Bình thường";
                else if (root.TryGetProperty("Finding", out var fPropCap)) finalResult.Finding = fPropCap.GetString() ?? "Bình thường";

                if (root.TryGetProperty("confidence", out var cProp)) finalResult.Confidence = GetDoubleSafe(cProp);
                else if (root.TryGetProperty("Confidence", out var cPropCap)) finalResult.Confidence = GetDoubleSafe(cPropCap);

                if (root.TryGetProperty("x", out var xProp)) finalResult.X = GetIntSafe(xProp);
                else if (root.TryGetProperty("X", out var xPropCap)) finalResult.X = GetIntSafe(xPropCap);

                if (root.TryGetProperty("y", out var yProp)) finalResult.Y = GetIntSafe(yProp);
                else if (root.TryGetProperty("Y", out var yPropCap)) finalResult.Y = GetIntSafe(yPropCap);

                if (root.TryGetProperty("width", out var wProp)) finalResult.Width = GetIntSafe(wProp);
                else if (root.TryGetProperty("Width", out var wPropCap)) finalResult.Width = GetIntSafe(wPropCap);

                if (root.TryGetProperty("height", out var hProp)) finalResult.Height = GetIntSafe(hProp);
                else if (root.TryGetProperty("Height", out var hPropCap)) finalResult.Height = GetIntSafe(hPropCap);

                if (root.TryGetProperty("quality", out var qProp)) finalResult.Quality = qProp.GetString() ?? "Đạt tiêu chuẩn";
                else if (root.TryGetProperty("Quality", out var qPropCap)) finalResult.Quality = qPropCap.GetString() ?? "Đạt tiêu chuẩn";

                if (root.TryGetProperty("lesion", out var lProp)) finalResult.Lesion = lProp.GetString() ?? "Không phát hiện";
                else if (root.TryGetProperty("Lesion", out var lPropCap)) finalResult.Lesion = lPropCap.GetString() ?? "Không phát hiện";

                if (root.TryGetProperty("alignment", out var aProp)) finalResult.Alignment = aProp.GetString() ?? "Bình thường";
                else if (root.TryGetProperty("Alignment", out var aPropCap)) finalResult.Alignment = aPropCap.GetString() ?? "Bình thường";

                if (root.TryGetProperty("softTissue", out var stProp)) finalResult.SoftTissue = stProp.GetString() ?? "Bình thường";
                else if (root.TryGetProperty("SoftTissue", out var stPropCap)) finalResult.SoftTissue = stPropCap.GetString() ?? "Bình thường";

                // Coordinate range safety check
                if (finalResult.X < 0 || finalResult.X > 1000 || finalResult.Y < 0 || finalResult.Y > 1000)
                {
                    log.ValidationError = $"Vision coordinates are out of bounds: X={finalResult.X}, Y={finalResult.Y}. Resetting coordinates to 0.";
                    finalResult.X = 0; finalResult.Y = 0; finalResult.Width = 0; finalResult.Height = 0;
                }
            }
            catch (Exception ex)
            {
                log.Success = false;
                log.ErrorMessage = ex.Message;
                throw;
            }
            finally
            {
                watch.Stop();
                log.ExecutionTimeMs = watch.Elapsed.TotalMilliseconds;
                AddLog(log);
            }

            return finalResult;
        }

        // --- MODULE 3: SUGGEST DEPARTMENT ---
        public async Task<int> SuggestDepartmentAIAsync(string symptoms, List<DepartmentDto> departments, string? customOpenAiKey = null, string? customGeminiKey = null)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var log = new AiRequestLogEntry
            {
                Module = "Department",
                Input = $"Symptoms: {symptoms}, Departments: {departments.Count}"
            };

            int fallbackDeptId = departments.FirstOrDefault()?.Id ?? 0;
            string deptsJson = JsonSerializer.Serialize(departments);

            string? openAiKey = GetOpenAiApiKey(customOpenAiKey);

            // 1. Try OpenAI if key is present
            if (!string.IsNullOrEmpty(openAiKey))
            {
                try
                {
                    string url = "https://api.openai.com/v1/chat/completions";
                    var payload = new
                    {
                        model = "gpt-4o-mini",
                        messages = new[]
                        {
                            new { role = "system", content = "Bạn là một bác sĩ hỗ trợ chọn chuyên khoa khám. Chỉ trả lời bằng đúng số ID nguyên duy nhất của chuyên khoa." },
                            new { role = "user", content = $"Một bệnh nhân có triệu chứng sau: '{symptoms}'. Bệnh viện có các chuyên khoa sau (định dạng JSON): {deptsJson}. Dựa vào triệu chứng, hãy chọn 1 chuyên khoa phù hợp nhất để khám. CHỈ TRẢ VỀ ID (số nguyên) CỦA CHUYÊN KHOA ĐÓ, không thêm bất kỳ văn bản, giải thích hay markdown nào khác." }
                        }
                    };

                    using var client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiKey}");
                    var reqContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = reqContent };

                    var response = await SendWithRetryAsync(client, request);
                    log.Success = response.IsSuccessStatusCode;
                    string responseString = await response.Content.ReadAsStringAsync();
                    log.RawResponse = responseString;

                    if (response.IsSuccessStatusCode)
                    {
                        using var doc = JsonDocument.Parse(responseString);
                        string? content = doc.RootElement
                            .GetProperty("choices")[0]
                            .GetProperty("message")
                            .GetProperty("content")
                            .GetString();

                        if (!string.IsNullOrEmpty(content))
                        {
                            string onlyDigits = new string(content.Trim().Where(char.IsDigit).ToArray());
                            if (int.TryParse(onlyDigits, out int deptId) && departments.Any(d => d.Id == deptId))
                            {
                                watch.Stop();
                                log.ExecutionTimeMs = watch.Elapsed.TotalMilliseconds;
                                AddLog(log);
                                return deptId;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Success = false;
                    log.ErrorMessage = "OpenAI failed: " + ex.Message;
                }
            }

            // 2. Fallback to Gemini
            try
            {
                string geminiKey = GetGeminiApiKey(customGeminiKey);
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={geminiKey}";

                var payload = new
                {
                    systemInstruction = new
                    {
                        parts = new object[] { new { text = "Bạn là bác sĩ phân loại bệnh nhân. Chỉ trả lời duy nhất số ID nguyên của chuyên khoa phù hợp." } }
                    },
                    contents = new object[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new { text = $"Chọn ID chuyên khoa phù hợp nhất cho triệu chứng '{symptoms}' từ danh sách này: {deptsJson}. Chỉ trả về duy nhất ID (Ví dụ: 3)." }
                            }
                        }
                    }
                };

                using var client = new HttpClient();
                var reqContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = reqContent };

                var response = await SendWithRetryAsync(client, request);
                log.Success = response.IsSuccessStatusCode;
                string responseString = await response.Content.ReadAsStringAsync();
                log.RawResponse = responseString;

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseString);
                    string? rawText = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    if (!string.IsNullOrEmpty(rawText))
                    {
                        string onlyDigits = new string(rawText.Trim().Where(char.IsDigit).ToArray());
                        if (int.TryParse(onlyDigits, out int deptId) && departments.Any(d => d.Id == deptId))
                        {
                            watch.Stop();
                            log.ExecutionTimeMs = watch.Elapsed.TotalMilliseconds;
                            AddLog(log);
                            return deptId;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Success = false;
                log.ErrorMessage = "Gemini fallback failed: " + ex.Message;
            }

            // 3. Heuristic local fallback
            string s = symptoms.ToLower();
            if (s.Contains("mỏi tay") || s.Contains("đau tay") || s.Contains("mỏi vai") || s.Contains("vai gáy") || s.Contains("đau lưng") || s.Contains("xương") || s.Contains("khớp") || s.Contains("gãy"))
            {
                var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("xương") || x.DepartmentName.ToLower().Contains("khớp") || x.DepartmentName.ToLower().Contains("ngoại"));
                if (d != null) fallbackDeptId = d.Id;
            }
            else if (s.Contains("ho") || s.Contains("sổ mũi") || s.Contains("đau họng") || s.Contains("tai ") || s.Contains("mũi "))
            {
                var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("tai") || x.DepartmentName.ToLower().Contains("họng") || x.DepartmentName.ToLower().Contains("nội"));
                if (d != null) fallbackDeptId = d.Id;
            }
            else if (s.Contains("răng") || s.Contains("nướu") || s.Contains("hàm"))
            {
                var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("răng") || x.DepartmentName.ToLower().Contains("hàm"));
                if (d != null) fallbackDeptId = d.Id;
            }
            else if (s.Contains("mắt") || s.Contains("mờ") || s.Contains("cận"))
            {
                var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("mắt"));
                if (d != null) fallbackDeptId = d.Id;
            }
            else if (s.Contains("đau đầu") || s.Contains("chóng mặt") || s.Contains("đau bụng") || s.Contains("buồn nôn") || s.Contains("sốt") || s.Contains("tim") || s.Contains("huyết áp"))
            {
                var d = departments.FirstOrDefault(x => x.DepartmentName.ToLower().Contains("nội") || x.DepartmentName.ToLower().Contains("tổng hợp") || x.DepartmentName.ToLower().Contains("tim"));
                if (d != null) fallbackDeptId = d.Id;
            }

            watch.Stop();
            log.ExecutionTimeMs = watch.Elapsed.TotalMilliseconds;
            log.ValidationError = "Không có API hoạt động hoặc kết quả API không hợp lệ. Đã kích hoạt Heuristic Fallback.";
            AddLog(log);
            return fallbackDeptId;
        }

        // --- MODULE 4: INTERACTIVE PATIENT CHAT ---
        public async Task<string> SendChatMessageAsync(string message, List<ChatMessageDto> history, string? customApiKey = null)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var log = new AiRequestLogEntry
            {
                Module = "Chat",
                Input = $"Message: {message}"
            };

            string reply = "Lỗi kết nối đến dịch vụ AI. Vui lòng thử lại sau.";

            try
            {
                string apiKey = GetGeminiApiKey(customApiKey);
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3.5-flash:generateContent?key={apiKey}";

                var contentsList = new List<object>();
                if (history != null)
                {
                    foreach (var h in history)
                    {
                        contentsList.Add(new
                        {
                            role = h.Role == "user" ? "user" : "model",
                            parts = new object[] { new { text = h.Content } }
                        });
                    }
                }

                contentsList.Add(new
                {
                    role = "user",
                    parts = new object[] { new { text = message } }
                });

                var payload = new
                {
                    systemInstruction = new
                    {
                        parts = new object[]
                        {
                            new { text = "Bạn là trợ lý AI Y tế của hệ thống QL KCB. YÊU CẦU BẮT BUỘC: Trả lời CỰC KỲ NGẮN GỌN, SÚC TÍCH và DỄ ĐỌC (dưới 120 từ, tối đa 3-4 gạch đầu dòng). Đi thẳng vào 2-3 gợi ý xử lý ban đầu và chỉ rõ chuyên khoa cần đăng ký khám nếu triệu chứng kéo dài. Tuyệt đối không tự ý kê đơn thuốc điều trị bệnh nặng trực tuyến. Luôn kết thúc bằng khuyên đi khám bác sĩ thực tế." }
                        }
                    },
                    contents = contentsList
                };

                using var client = new HttpClient();
                var reqContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = reqContent };

                var response = await SendWithRetryAsync(client, request);
                log.Success = response.IsSuccessStatusCode;
                string responseString = await response.Content.ReadAsStringAsync();
                log.RawResponse = responseString;

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseString);
                    reply = doc.RootElement
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text").GetString() ?? "";

                    if (string.IsNullOrEmpty(reply))
                    {
                        reply = "Không nhận được phản hồi từ mô hình AI.";
                        log.ValidationError = "AI text content is null.";
                    }
                }
                else
                {
                    log.ErrorMessage = $"Chat API status {response.StatusCode}, Content: {responseString}";
                }
            }
            catch (Exception ex)
            {
                log.Success = false;
                log.ErrorMessage = ex.Message;
            }
            finally
            {
                watch.Stop();
                log.ExecutionTimeMs = watch.Elapsed.TotalMilliseconds;
                AddLog(log);
            }

            return reply;
        }

        // --- AUTOMATED TESTING: CLINICAL TEST SUITE RUNNER ---
        public async Task<List<TestScenarioResult>> RunClinicalTestSuiteAsync(List<ICD10Protocol> databaseProtocols, string? customApiKey = null)
        {
            var testScenarios = new List<dynamic>
            {
                new { Name = "Ca 1: Viêm đường hô hấp cấp", Symptoms = "Sốt cao, ho khạc đờm vàng, đau rát cổ họng, chảy nước mũi trong 3 ngày nay.", ExpectedIcdGroup = "J" },
                new { Name = "Ca 2: Tăng huyết áp lâm sàng", Symptoms = "Đau đầu dữ dội vùng sau gáy, hoa mắt, chóng mặt kèm cảm giác tức ngực nhẹ, đo huyết áp tại nhà 160/95 mmHg.", ExpectedIcdGroup = "I" },
                new { Name = "Ca 3: Đái tháo đường khởi phát", Symptoms = "Thường xuyên mệt mỏi, sút cân nhanh không rõ nguyên nhân, ăn uống nhiều, tiểu nhiều và rất nhanh khát nước.", ExpectedIcdGroup = "E" },
                new { Name = "Ca 4: Viêm dạ dày tá tràng", Symptoms = "Đau rát âm ỉ vùng thượng vị xuất hiện sau khi ăn, thường xuyên ợ hơi, ợ chua và có cảm giác buồn nôn nhẹ.", ExpectedIcdGroup = "K" },
                new { Name = "Ca 5: Rối loạn khớp", Symptoms = "Sưng đau khớp gối bên phải, đi lại vận động khó khăn, cứng khớp vào buổi sáng kéo dài 30 phút.", ExpectedIcdGroup = "M" }
            };

            var results = new List<TestScenarioResult>();

            foreach (var sc in testScenarios)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var result = new TestScenarioResult
                {
                    CaseName = sc.Name,
                    InputSymptoms = sc.Symptoms,
                    ExpectedIcdGroup = sc.ExpectedIcdGroup
                };

                try
                {
                    var response = await PredictIcdFromSymptomsAsync(sc.Symptoms, databaseProtocols, customApiKey);
                    result.PredictedIcd = response.IcdCode;
                    result.PredictedDiagnosis = response.Diagnosis;
                    
                    // Validation: predicted ICD code starts with expected letter group (e.g. 'J' or 'I' or 'E')
                    if (!string.IsNullOrEmpty(response.IcdCode) && response.IcdCode.StartsWith(sc.ExpectedIcdGroup, StringComparison.OrdinalIgnoreCase))
                    {
                        result.Passed = true;
                    }
                    else
                    {
                        result.Passed = false;
                        result.ErrorDetails = $"Mã ICD '{response.IcdCode}' không thuộc nhóm dự kiến '{sc.ExpectedIcdGroup}'.";
                    }
                }
                catch (Exception ex)
                {
                    result.Passed = false;
                    result.ErrorDetails = ex.Message;
                }
                finally
                {
                    watch.Stop();
                    result.LatencyMs = watch.Elapsed.TotalMilliseconds;
                    results.Add(result);
                }
            }

            return results;
        }
    }
}
