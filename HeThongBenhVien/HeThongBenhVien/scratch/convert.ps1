$path = "C:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\HeThongBenhVien\scratch\generate_medicines.ps1"
$content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
$utf8WithBom = New-Object System.Text.UTF8Encoding($true)
[System.IO.File]::WriteAllText($path, $content, $utf8WithBom)
Write-Host "Successfully converted generate_medicines.ps1 to UTF-8 with BOM."
