using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace HeThongBenhVien.Hubs
{
    public class MedicalCommandHub : Hub
    {
        // Nhận tín hiệu từ Monitor giường bệnh gửi về máy chủ
        public async Task SendVitalsUpdate(string bedId, int heartRate, int spO2, string bloodPressure, int ewsScore)
        {
            // Trực tiếp phát lại dữ liệu này tới toàn bộ các Client (bảng điều khiển)
            await Clients.All.SendAsync("ReceiveVitalsUpdate", bedId, heartRate, spO2, bloodPressure, ewsScore);
        }

        // Tín hiệu kích hoạt báo động đỏ Code Blue
        public async Task SendCodeBlue(string bedId, string bedNum, string deptName)
        {
            await Clients.All.SendAsync("ReceiveCodeBlue", bedId, bedNum, deptName);
        }
    }
}
