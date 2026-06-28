$files = @(
    "Views/Doctor/ChiTietBenhAn.cshtml",
    "Views/Doctor/DanhSach.cshtml",
    "Views/Doctor/Dashboard.cshtml",
    "Views/Doctor/HenTaiKham.cshtml",
    "Views/Doctor/HoSoBenhAn.cshtml",
    "Views/Doctor/KeDonThuoc.cshtml",
    "Views/Doctor/KeDonThuocForm.cshtml",
    "Views/Doctor/LichSuDieuTri.cshtml",
    "Views/Shared/_DoctorLayout.cshtml",
    "Views/Shared/_AdminLayout.cshtml",
    "Models/DoctorDashboardViewModel.cs",
    "Controllers/DoctorController.cs"
)

foreach ($f in $files) {
    $p = Join-Path "c:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\HeThongBenhVien" $f
    if (Test-Path $p) {
        $content = [System.IO.File]::ReadAllText($p, [System.Text.Encoding]::UTF8)
        [System.IO.File]::WriteAllText($p, $content, [System.Text.Encoding]::UTF8)
        Write-Output "Converted $f to UTF-8 BOM"
    }
}
