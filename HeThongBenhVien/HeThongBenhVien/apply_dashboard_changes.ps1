$projectRoot = "c:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\HeThongBenhVien"
$viewPath = Join-Path $projectRoot "Views\Doctor\Dashboard.cshtml"

Write-Output "Reading Dashboard.cshtml..."
$lines = [System.IO.File]::ReadAllLines($viewPath, [System.Text.Encoding]::UTF8)

$startIdx = -1
$endIdx = -1
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -like "*Header Section*") {
        $startIdx = $i
    }
    if ($lines[$i] -like "*id=`"identifyPatientModal`"*") {
        if ($lines[$i-1] -like "*Modal*") {
            $endIdx = $i - 1
        } else {
            $endIdx = $i
        }
        break
    }
}

if ($startIdx -eq -1 -or $endIdx -eq -1) {
    Write-Error "Markers not found in Dashboard.cshtml!"
    exit 1
}

Write-Output "Found markers at $startIdx and $endIdx."

$newUi = @'
    <style>
        .hover-scale {
            transition: transform 0.2s, box-shadow 0.2s;
        }
        .hover-scale:hover {
            transform: translateY(-2px);
        }
        .hover-scale:hover .shadow-sm {
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06) !important;
        }
        .bg-soft-success {
            background-color: #d1fae5 !important;
            color: #065f46 !important;
        }
        .bg-soft-danger {
            background-color: #fee2e2 !important;
            color: #991b1b !important;
        }
        .bg-soft-info {
            background-color: #e0f2fe !important;
            color: #0369a1 !important;
        }
        .bg-soft-warning {
            background-color: #fef3c7 !important;
            color: #92400e !important;
        }
        .no-arrow::after {
            display: none !important;
        }
    </style>

    <!-- Alerts Row -->
    <div class="row mb-4 g-3">
        <!-- Red emergency alert -->
        <div class="col-lg-6">
            @{
                var hasCriticalLab = Model.LabAlerts.Any();
                var criticalVitals = Model.VitalAlerts.Where(v => 
                    (double.TryParse(v.Pulse, out double pulse) && pulse < 40) ||
                    (double.TryParse(v.SpO2, out double spo2) && spo2 < 85)
                ).ToList();
                var hasCriticalVitals = criticalVitals.Any();
            }
            @if (hasCriticalLab)
            {
                var labAlert = Model.LabAlerts.First();
                <div class="p-3 d-flex align-items-center justify-content-between h-100 animate__animated animate__pulse animate__infinite" style="background-color: #fee2e2; border: 1px solid #fca5a5; border-radius: 12px;">
                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center justify-content-center text-white rounded-circle" style="width: 36px; height: 36px; min-width: 36px; background-color: #ef4444 !important;">
                            <i class="fa-solid fa-circle-exclamation" style="font-size: 16px;"></i>
                        </div>
                        <div>
                            <h6 class="mb-1 fw-bold text-danger" style="font-size: 0.95rem; color: #991b1b !important;">Cảnh báo khẩn cấp (@Model.LabAlerts.Count.ToString("D2"))</h6>
                            <p class="mb-0 text-muted" style="font-size: 0.85rem; color: #7f1d1d !important; line-height: 1.35;">Bệnh nhân @(labAlert.MedicalRecord?.Appointment?.Patient?.FullName) (Mã: @(labAlert.MedicalRecord?.Appointment?.Patient?.PatientCode)) có kết quả @(labAlert.TestName): @(labAlert.Result). Cần xử lý ngay.</p>
                        </div>
                    </div>
                    <a href="@Url.Action("ChiTietBenhAn", "Doctor", new { id = labAlert.MedicalRecordId })" class="btn btn-danger btn-sm fw-semibold px-3 py-2 ms-3" style="border-radius: 8px; background-color: #b91c1c; border: none; font-size: 0.8rem; white-space: nowrap;">Xử lý ngay</a>
                </div>
            }
            else if (hasCriticalVitals)
            {
                var vitalAlert = criticalVitals.First();
                <div class="p-3 d-flex align-items-center justify-content-between h-100 animate__animated animate__pulse animate__infinite" style="background-color: #fee2e2; border: 1px solid #fca5a5; border-radius: 12px;">
                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center justify-content-center text-white rounded-circle" style="width: 36px; height: 36px; min-width: 36px; background-color: #ef4444 !important;">
                            <i class="fa-solid fa-circle-exclamation" style="font-size: 16px;"></i>
                        </div>
                        <div>
                            <h6 class="mb-1 fw-bold text-danger" style="font-size: 0.95rem; color: #991b1b !important;">Cảnh báo khẩn cấp (@criticalVitals.Count.ToString("D2"))</h6>
                            <p class="mb-0 text-muted" style="font-size: 0.85rem; color: #7f1d1d !important; line-height: 1.35;">Bệnh nhân @(vitalAlert.Appointment?.Patient?.FullName) có dấu hiệu nguy kịch. Mạch @vitalAlert.Pulse l/p, SpO2 @vitalAlert.SpO2%. Yêu cầu hỗ trợ khẩn cấp!</p>
                        </div>
                    </div>
                    <a href="@Url.Action("SinhHieuChiTiet", "Doctor", new { patientId = vitalAlert.Appointment?.PatientId })" class="btn btn-danger btn-sm fw-semibold px-3 py-2 ms-3" style="border-radius: 8px; background-color: #b91c1c; border: none; font-size: 0.8rem; white-space: nowrap;">Xử lý ngay</a>
                </div>
            }
            else if (Model.OverdueWaitingCount > 0)
            {
                <div class="p-3 d-flex align-items-center justify-content-between h-100" style="background-color: #fee2e2; border: 1px solid #fca5a5; border-radius: 12px;">
                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center justify-content-center text-white rounded-circle" style="width: 36px; height: 36px; min-width: 36px; background-color: #ef4444 !important;">
                            <i class="fa-solid fa-circle-exclamation" style="font-size: 16px;"></i>
                        </div>
                        <div>
                            <h6 class="mb-1 fw-bold text-danger" style="font-size: 0.95rem; color: #991b1b !important;">Cảnh báo khẩn cấp (@Model.OverdueWaitingCount.ToString("D2"))</h6>
                            <p class="mb-0 text-muted" style="font-size: 0.85rem; color: #7f1d1d !important; line-height: 1.35;">Có @Model.OverdueWaitingCount bệnh nhân đã chờ khám quá 30 phút. Vui lòng ưu tiên khám ngay!</p>
                        </div>
                    </div>
                    <a href="@Url.Action("DanhSach", "Doctor")" class="btn btn-danger btn-sm fw-semibold px-3 py-2 ms-3" style="border-radius: 8px; background-color: #b91c1c; border: none; font-size: 0.8rem; white-space: nowrap;">Xử lý ngay</a>
                </div>
            }
            else
            {
                <div class="p-3 d-flex align-items-center h-100" style="background-color: #ecfdf5; border: 1px solid #a7f3d0; border-radius: 12px;">
                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center justify-content-center text-white rounded-circle" style="width: 36px; height: 36px; min-width: 36px; background-color: #10b981 !important;">
                            <i class="fa-solid fa-circle-check" style="font-size: 16px;"></i>
                        </div>
                        <div>
                            <h6 class="mb-1 fw-bold text-success" style="font-size: 0.95rem; color: #065f46 !important;">Trạng thái phòng khám</h6>
                            <p class="mb-0 text-muted" style="font-size: 0.85rem; color: #047857 !important; line-height: 1.35;">Không có bệnh nhân nào chờ khám quá giờ. Mọi hoạt động diễn ra ổn định.</p>
                        </div>
                    </div>
                </div>
            }
        </div>
        <!-- Yellow warning alert -->
        <div class="col-lg-6">
            @{
                var tempAlerts = Model.VitalAlerts.Where(v => double.TryParse(v.Temperature, out double t) && t >= 39.0).ToList();
                var hasTempAlert = tempAlerts.Any();
            }
            @if (hasTempAlert)
            {
                var tempAlert = tempAlerts.First();
                <div class="p-3 d-flex align-items-center justify-content-between h-100" style="background-color: #fef3c7; border: 1px solid #fde68a; border-radius: 12px;">
                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center justify-content-center text-white rounded-circle" style="width: 36px; height: 36px; min-width: 36px; background-color: #f59e0b !important;">
                            <i class="fa-solid fa-triangle-exclamation" style="font-size: 16px;"></i>
                        </div>
                        <div>
                            <h6 class="mb-1 fw-bold text-warning" style="font-size: 0.95rem; color: #b45309 !important;">Lời nhắc quan trọng (@tempAlerts.Count.ToString("D2"))</h6>
                            <p class="mb-0 text-muted" style="font-size: 0.85rem; color: #78350f !important; line-height: 1.35;">Bệnh nhân @(tempAlert.Appointment?.Patient?.FullName) sốt cao: @(tempAlert.Temperature)°C. Vui lòng kiểm tra ngay.</p>
                        </div>
                    </div>
                    <a href="@Url.Action("SinhHieuChiTiet", "Doctor", new { patientId = tempAlert.Appointment?.PatientId })" class="btn btn-warning btn-sm fw-semibold px-3 py-2 ms-3 text-white" style="border-radius: 8px; background-color: #d97706; border: none; font-size: 0.8rem; white-space: nowrap;">Xem sinh hiệu</a>
                </div>
            }
            else if (Model.WaitingPrescriptionCount > 0)
            {
                <div class="p-3 d-flex align-items-center justify-content-between h-100" style="background-color: #fef3c7; border: 1px solid #fde68a; border-radius: 12px;">
                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center justify-content-center text-white rounded-circle" style="width: 36px; height: 36px; min-width: 36px; background-color: #f59e0b !important;">
                            <i class="fa-solid fa-triangle-exclamation" style="font-size: 16px;"></i>
                        </div>
                        <div>
                            <h6 class="mb-1 fw-bold text-warning" style="font-size: 0.95rem; color: #b45309 !important;">Lời nhắc quan trọng (@Model.WaitingPrescriptionCount.ToString("D2"))</h6>
                            <p class="mb-0 text-muted" style="font-size: 0.85rem; color: #78350f !important; line-height: 1.35;">Có @Model.WaitingPrescriptionCount bệnh nhân chờ kê toa thuốc — cần bác sĩ kê đơn sớm.</p>
                        </div>
                    </div>
                    <a href="@Url.Action("KeDonThuoc", "Doctor")" class="btn btn-warning btn-sm fw-semibold px-3 py-2 ms-3 text-white" style="border-radius: 8px; background-color: #d97706; border: none; font-size: 0.8rem; white-space: nowrap;">Kê đơn</a>
                </div>
            }
            else
            {
                <div class="p-3 d-flex align-items-center h-100" style="background-color: #eff6ff; border: 1px solid #bfdbfe; border-radius: 12px;">
                    <div class="d-flex align-items-center gap-3">
                        <div class="d-flex align-items-center justify-content-center text-white rounded-circle" style="width: 36px; height: 36px; min-width: 36px; background-color: #3b82f6 !important;">
                            <i class="fa-solid fa-circle-info" style="font-size: 16px;"></i>
                        </div>
                        <div>
                            <h6 class="mb-1 fw-bold text-primary" style="font-size: 0.95rem; color: #1e3a8a !important;">Lời nhắc y tế</h6>
                            <p class="mb-0 text-muted" style="font-size: 0.85rem; color: #1d4ed8 !important; line-height: 1.35;">Tất cả bệnh nhân khám xong đã được kê toa đầy đủ. Không có lịch chờ.</p>
                        </div>
                    </div>
                </div>
            }
        </div>
    </div>

    <!-- Summary Cards -->
    <div class="row mb-4 g-3">
        <!-- Chờ khám -->
        <div class="col-xl-4 col-md-6">
            <div class="glass-card p-4 d-flex justify-content-between align-items-start" style="border: 1px solid #e2e8f0; border-radius: 16px; background: white; box-shadow: 0 1px 3px rgba(0,0,0,0.02); position: relative; height: 100%;">
                <div>
                    <div class="d-flex align-items-center gap-2 mb-3">
                        <div class="rounded-circle d-flex align-items-center justify-content-center" style="width: 36px; height: 36px; background-color: rgba(14, 165, 233, 0.1) !important; color: #0284c7 !important;">
                            <i class="fa-regular fa-calendar-check" style="font-size: 16px;"></i>
                        </div>
                        <span class="text-muted fw-semibold" style="font-size: 0.85rem;">Chờ khám</span>
                    </div>
                    <div class="h2 mb-1 fw-bold text-dark" style="font-weight: 800;">@Model.TodayPatientsCount.ToString("D2")</div>
                    <small class="text-muted" style="font-size: 0.75rem;">Dự kiến hoàn thành trong 3h tới</small>
                </div>
                <span class="badge fw-bold px-2 py-1 rounded" style="font-size: 0.75rem; background-color: #d1fae5; color: #065f46 !important; position: absolute; top: 24px; right: 24px;">
                    <i class="fa-solid fa-arrow-up me-1" style="font-size: 10px;"></i> +12%
                </span>
            </div>
        </div>
        
        <!-- Đã hoàn thành -->
        <div class="col-xl-4 col-md-6">
            <div class="glass-card p-4 d-flex justify-content-between align-items-start" style="border: 1px solid #e2e8f0; border-radius: 16px; background: white; box-shadow: 0 1px 3px rgba(0,0,0,0.02); position: relative; height: 100%;">
                <div>
                    <div class="d-flex align-items-center gap-2 mb-3">
                        <div class="rounded-circle d-flex align-items-center justify-content-center" style="width: 36px; height: 36px; background-color: rgba(16, 185, 129, 0.1) !important; color: #059669 !important;">
                            <i class="fa-regular fa-circle-check" style="font-size: 16px;"></i>
                        </div>
                        <span class="text-muted fw-semibold" style="font-size: 0.85rem;">Đã hoàn thành</span>
                    </div>
                    <div class="h2 mb-1 fw-bold text-dark" style="font-weight: 800;">@Model.CompletedPatientsCount.ToString("D2")</div>
                    <small class="text-muted" style="font-size: 0.75rem;">So với trung bình 44 bệnh nhân/ngày</small>
                </div>
                <span class="badge fw-bold px-2 py-1 rounded" style="font-size: 0.75rem; background-color: #d1fae5; color: #065f46 !important; position: absolute; top: 24px; right: 24px;">
                    <i class="fa-solid fa-arrow-up me-1" style="font-size: 10px;"></i> +8%
                </span>
            </div>
        </div>

        <!-- Chưa xác nhận -->
        <div class="col-xl-4 col-md-6">
            <div class="glass-card p-4 d-flex justify-content-between align-items-start" style="border: 1px solid #e2e8f0; border-radius: 16px; background: white; box-shadow: 0 1px 3px rgba(0,0,0,0.02); position: relative; height: 100%;">
                <div>
                    <div class="d-flex align-items-center gap-2 mb-3">
                        <div class="rounded-circle d-flex align-items-center justify-content-center" style="width: 36px; height: 36px; background-color: rgba(245, 158, 11, 0.1) !important; color: #d97706 !important;">
                            <i class="fa-regular fa-calendar-plus" style="font-size: 16px;"></i>
                        </div>
                        <span class="text-muted fw-semibold" style="font-size: 0.85rem;">Chưa xác nhận</span>
                    </div>
                    <div class="h2 mb-1 fw-bold text-dark" style="font-weight: 800;">@Model.WaitingResultsCount.ToString("D2")</div>
                    <small class="text-muted" style="font-size: 0.75rem;">Yêu cầu lịch hẹn mới cần duyệt</small>
                </div>
                <span class="badge fw-bold px-2 py-1 rounded" style="font-size: 0.75rem; background-color: #fee2e2; color: #991b1b !important; position: absolute; top: 24px; right: 24px;">
                    <i class="fa-solid fa-arrow-down me-1" style="font-size: 10px;"></i> -2%
                </span>
            </div>
        </div>
    </div>

    <!-- Main Content Grid -->
    <div class="row g-4">
        <!-- CỘT TRÁI (8/12) -->
        <div class="col-xl-8 col-lg-7">
            
            <!-- CARD 1: Thống kê lưu lượng bệnh nhân -->
            <div class="glass-card p-4 mb-4" style="border: 1px solid #e2e8f0; border-radius: 16px; background: white; box-shadow: 0 1px 3px rgba(0,0,0,0.02);">
                <div class="d-flex justify-content-between align-items-center mb-4">
                    <div>
                        <h5 class="fw-bold mb-0 text-dark" style="font-size: 1.1rem;">Thống kê lưu lượng bệnh nhân</h5>
                        <small class="text-muted" style="font-size: 0.8rem;">Dữ liệu tổng hợp 7 ngày qua</small>
                    </div>
                    <div class="d-flex gap-3 align-items-center" style="font-size: 0.8rem; font-weight: 500;">
                        <span class="d-flex align-items-center gap-1"><span class="rounded-circle" style="width: 8px; height: 8px; background-color: #0ea5e9; display: inline-block;"></span> Bệnh nhân</span>
                        <span class="d-flex align-items-center gap-1"><span class="rounded-circle" style="width: 8px; height: 8px; background-color: #38bdf8; display: inline-block;"></span> Chỉ số PT</span>
                    </div>
                </div>
                <div style="position: relative; height: 260px; width: 100%;">
                    <canvas id="patientsChart"></canvas>
                </div>
            </div>

            <!-- CARD 2: Lịch hẹn gần đây -->
            <div class="glass-card p-4" style="border: 1px solid #e2e8f0; border-radius: 16px; background: white; box-shadow: 0 1px 3px rgba(0,0,0,0.02);">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h5 class="fw-bold mb-0 text-dark" style="font-size: 1.1rem;">Lịch hẹn gần đây</h5>
                    <a href="@Url.Action("DanhSach", "Doctor")" class="btn btn-sm btn-outline-primary px-3 py-1.5 fw-semibold" style="border-radius: 8px; font-size: 0.75rem; border-color: rgba(2, 132, 199, 0.2); color: #0284c7;">Xem tất cả</a>
                </div>
                
                <div class="table-responsive">
                    <table class="table align-middle mb-0" style="font-size: 13.5px;">
                        <thead>
                            <tr style="color: #64748b; font-weight: 600; border-bottom: 2px solid #f1f5f9; font-size: 0.75rem; text-transform: uppercase;">
                                <th class="ps-0 py-3 border-0">Bệnh nhân</th>
                                <th class="py-3 border-0">Thời gian</th>
                                <th class="py-3 border-0">Dịch vụ</th>
                                <th class="py-3 border-0">Trạng thái</th>
                                <th class="pe-0 py-3 border-0 text-end">Thao tác</th>
                            </tr>
                        </thead>
                        <tbody>
                            @if (!Model.UpcomingAppointments.Any())
                            {
                                <tr>
                                    <td colspan="5" class="text-center py-4 text-muted">Không có lịch hẹn chờ khám nào.</td>
                                </tr>
                            }
                            else
                            {
                                @foreach (var item in Model.UpcomingAppointments.Take(5))
                                {
                                    string nameParts = item.Patient?.FullName?.Trim() ?? "Unknown";
                                    string initials = "BN";
                                    if (!string.IsNullOrEmpty(nameParts))
                                    {
                                        var words = nameParts.Split(' ');
                                        if (words.Length > 1) {
                                            initials = "" + words[0][0] + words[words.Length-1][0];
                                        } else {
                                            initials = nameParts.Substring(0, System.Math.Min(2, nameParts.Length));
                                        }
                                    }
                                    initials = initials.ToUpper();

                                    string bgCircleColor = item.Status == 0 ? "#f43f5e" : item.Status == 1 ? "#0ea5e9" : "#a855f7";

                                    <tr style="border-bottom: 1px solid #f1f5f9;">
                                        <td class="ps-0 py-3">
                                            <div class="d-flex align-items-center">
                                                <div class="rounded-circle d-flex align-items-center justify-content-center text-white fw-bold me-2" style="width: 32px; height: 32px; background-color: @bgCircleColor; font-size: 11px;">@initials</div>
                                                <div>
                                                    <div class="fw-semibold text-dark">@item.Patient?.FullName</div>
                                                    <small class="text-muted" style="font-size: 11px;">ID: @item.Patient?.PatientCode</small>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="py-3 fw-medium text-dark">@item.AppointmentTime.ToString("hh:mm tt")</td>
                                        <td class="py-3 text-muted">@item.Reason</td>
                                        <td class="py-3">
                                            @if (item.Status == AppointmentStatus.ChoKham)
                                            {
                                                <span class="badge px-2.5 py-1 rounded" style="background-color: #e0f2fe; color: #0369a1; font-weight: 600; font-size: 0.75rem;">CHỜ KHÁM</span>
                                            }
                                            else if (item.Status == AppointmentStatus.ChuaDen)
                                            {
                                                <span class="badge px-2.5 py-1 rounded" style="background-color: #ffe4e6; color: #b91c1c; font-weight: 600; font-size: 0.75rem;">HẸN MỚI</span>
                                            }
                                            else
                                            {
                                                <span class="badge px-2.5 py-1 rounded" style="background-color: #f1f5f9; color: #475569; font-weight: 600; font-size: 0.75rem;">@AppointmentStatus.GetLabel(item.Status).ToUpper()</span>
                                            }
                                        </td>
                                        <td class="pe-0 py-3 text-end" style="white-space: nowrap;">
                                            <a href="@Url.Action("KhamBenh", "Doctor", new { id = item.Id })" class="btn btn-sm btn-light border fw-semibold me-1 px-2.5 py-1" style="font-size: 0.75rem; border-radius: 6px;">Khám</a>
                                            <div class="dropdown d-inline-block">
                                                <button class="btn btn-link text-muted p-0 dropdown-toggle no-arrow" type="button" data-bs-toggle="dropdown">
                                                    <i class="fa-solid fa-ellipsis-vertical"></i>
                                                </button>
                                                <ul class="dropdown-menu dropdown-menu-end border shadow-sm" style="font-size: 13px;">
                                                    <li><a class="dropdown-item" href="@Url.Action("ChiTietByAppointment", "Doctor", new { id = item.Id })">Chi tiết</a></li>
                                                    <li><a class="dropdown-item text-danger" href="#">Hủy lịch</a></li>
                                                </ul>
                                            </div>
                                        </td>
                                    </tr>
                                }
                            }
                        </tbody>
                    </table>
                </div>
            </div>

        </div>

        <!-- CỘT PHẢI (4/12) -->
        <div class="col-xl-4 col-lg-5">
            
            <!-- CARD 1: Chức năng y tế -->
            <div class="glass-card p-4 mb-4" style="border: 1px solid #e2e8f0; border-radius: 16px; background: white; box-shadow: 0 1px 3px rgba(0,0,0,0.02);">
                <h5 class="fw-bold mb-4 text-dark" style="font-size: 1.1rem;">Chức năng y tế</h5>
                
                <div class="row g-4 row-cols-3">
                    
                    <!-- ĐK bệnh nhân -->
                    <div class="col text-center">
                        <a href="@Url.Action("DanhSach", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #3b82f6;">
                                <i class="fa-solid fa-users" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">ĐK bệnh nhân</div>
                        </a>
                    </div>

                    <!-- Tiếp nhận mới -->
                    <div class="col text-center">
                        <a href="#" data-bs-toggle="modal" data-bs-target="#addAppointmentModal" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #06b6d4;">
                                <i class="fa-solid fa-user-plus" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Tiếp nhận Mới</div>
                        </a>
                    </div>

                    <!-- Xác Nhận Bệnh Nhân (Face/QR scan) -->
                    <div class="col text-center">
                        <a href="#" data-bs-toggle="modal" data-bs-target="#identifyPatientModal" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #8b5cf6;">
                                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" style="width: 20px; height: 20px;">
                                    <path d="M3 7V5a2 2 0 0 1 2-2h2"></path>
                                    <path d="M17 3h2a2 2 0 0 1 2 2v2"></path>
                                    <path d="M21 17v2a2 2 0 0 1-2 2h-2"></path>
                                    <path d="M7 21H5a2 2 0 0 1-2-2v-2"></path>
                                    <path d="M9 10a3 3 0 1 1 6 0v2a3 3 0 0 1-6 0v-2z"></path>
                                    <path d="M6 20a6 6 0 0 1 12 0"></path>
                                    <line x1="2" y1="12" x2="22" y2="12" stroke="#f87171" stroke-width="1.8"></line>
                                </svg>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Xác Nhận BN</div>
                        </a>
                    </div>

                    <!-- Lịch Hẹn -->
                    <div class="col text-center">
                        <a href="@Url.Action("LichHen", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #64748b;">
                                <i class="fa-solid fa-calendar-days" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Lịch Hẹn</div>
                        </a>
                    </div>

                    <!-- Hồ sơ BA -->
                    <div class="col text-center">
                        <a href="@Url.Action("HoSoBenhAn", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #8b5cf6;">
                                <i class="fa-solid fa-notes-medical" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Hồ sơ BA</div>
                        </a>
                    </div>

                    <!-- Kê đơn thuốc -->
                    <div class="col text-center">
                        <a href="@Url.Action("KeDonThuoc", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #10b981;">
                                <i class="fa-solid fa-pills" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Kê đơn Thuốc</div>
                        </a>
                    </div>

                    <!-- Chỉ định XN -->
                    <div class="col text-center">
                        <a href="@Url.Action("ChiDinhXetNghiem", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #3b82f6;">
                                <i class="fa-solid fa-flask" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Chỉ định XN</div>
                        </a>
                    </div>

                    <!-- Kết quả CLS -->
                    <div class="col text-center">
                        <a href="@Url.Action("KetQuaCanLamSang", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #3b82f6;">
                                <i class="fa-solid fa-microscope" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Kết quả CLS</div>
                        </a>
                    </div>

                    <!-- Sinh hiệu -->
                    <div class="col text-center">
                        <a href="@Url.Action("SinhHieu", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #10b981;">
                                <i class="fa-solid fa-heart-pulse" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Sinh hiệu</div>
                        </a>
                    </div>

                    <!-- Lịch sử khám -->
                    <div class="col text-center">
                        <a href="@Url.Action("LichSuKham", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #64748b;">
                                <i class="fa-solid fa-clock-rotate-left" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Lịch sử khám</div>
                        </a>
                    </div>

                    <!-- Lịch Mổ -->
                    <div class="col text-center">
                        <a href="@Url.Action("LichMo", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #64748b;">
                                <i class="fa-solid fa-scalpel" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Lịch Mổ</div>
                        </a>
                    </div>

                    <!-- Cảnh báo TT -->
                    <div class="col text-center">
                        <a href="@Url.Action("CanhBaoTinhTrang", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #ef4444;">
                                <i class="fa-solid fa-triangle-exclamation" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Cảnh báo TT</div>
                        </a>
                    </div>

                    <!-- Hẹn tái khám -->
                    <div class="col text-center">
                        <a href="@Url.Action("HenTaiKham", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #3b82f6;">
                                <i class="fa-solid fa-clock-rotate-left" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Hẹn tái khám</div>
                        </a>
                    </div>

                    <!-- QL Giường -->
                    <div class="col text-center">
                        <a href="@Url.Action("QuanLyGiuong", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #64748b;">
                                <i class="fa-solid fa-bed" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">QL Giường</div>
                        </a>
                    </div>

                    <!-- Hội chẩn Online -->
                    <div class="col text-center">
                        <a href="@Url.Action("HoiChanOnline", "Doctor")" class="d-block text-decoration-none hover-scale">
                            <div class="mx-auto rounded-circle d-flex align-items-center justify-content-center text-white mb-2 shadow-sm" style="width: 48px; height: 48px; background-color: #06b6d4;">
                                <i class="fa-solid fa-video" style="font-size: 16px;"></i>
                            </div>
                            <div style="font-size: 11px; color: #475569; font-weight: 500; line-height: 1.25;">Hội chẩn Online</div>
                        </a>
                    </div>

                </div>
            </div>

            <!-- CARD 2: Công việc trong ngày -->
            <div class="glass-card p-4 shadow-sm" style="border: 1px solid #e2e8f0; border-radius: 16px; background: white; box-shadow: 0 1px 3px rgba(0,0,0,0.02);">
                <div class="d-flex justify-content-between align-items-center mb-3">
                    <h5 class="fw-bold mb-0 text-dark" style="font-size: 1.1rem;">Công việc trong ngày</h5>
                    <span class="badge text-primary fw-bold px-2 py-1 rounded" style="font-size: 0.75rem; background-color: #e0f2fe; color: #0369a1 !important;">HÔM NAY</span>
                </div>
                
                <div class="d-flex flex-column gap-3">
                    
                    <!-- Phẫu thuật -->
                    <div class="d-flex align-items-center justify-content-between p-3" style="background-color: #f8fafc; border-radius: 12px; border: 1px solid #f1f5f9;">
                        <div class="d-flex align-items-center gap-2">
                            <i class="fa-solid fa-scissors text-muted" style="font-size: 16px;"></i>
                            <span class="fw-semibold text-dark" style="font-size: 0.85rem;">Phẫu thuật</span>
                        </div>
                        <span class="fw-bold text-dark" style="font-size: 1.1rem;">@Model.TodaySurgeries.Count</span>
                    </div>

                    <!-- Tái khám -->
                    <div class="d-flex align-items-center justify-content-between p-3" style="background-color: #f8fafc; border-radius: 12px; border: 1px solid #f1f5f9;">
                        <div class="d-flex align-items-center gap-2">
                            <i class="fa-solid fa-stethoscope text-muted" style="font-size: 16px;"></i>
                            <span class="fw-semibold text-dark" style="font-size: 0.85rem;">Tái khám</span>
                        </div>
                        <span class="fw-bold text-dark" style="font-size: 1.1rem;">@Model.TodayFollowUps.Count</span>
                    </div>

                </div>
            </div>

        </div>
    </div>
'@

# Reconstruct view lines
$updatedLines = $lines[0..($startIdx-1)] + $newUi + $lines[$endIdx..($lines.Count-1)]

[System.IO.File]::WriteAllLines($viewPath, $updatedLines, [System.Text.Encoding]::UTF8)
Write-Output "Dashboard.cshtml updated successfully."
Write-Output "ALL UPDATES COMPLETE."
