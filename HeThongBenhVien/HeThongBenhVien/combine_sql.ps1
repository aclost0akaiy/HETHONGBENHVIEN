$header = "USE master;
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyBenhVienDb')
BEGIN
    CREATE DATABASE QuanLyBenhVienDb;
END
GO
USE QuanLyBenhVienDb;
GO
"
$ef = Get-Content -Path "EF_Clean.sql" -Raw -Encoding UTF8
$data = Get-Content -Path "Data_Only.sql" -Raw -Encoding UTF8
$final = $header + "
" + $ef + "
" + $data

# Clean up multiple empty lines
$final = [System.Text.RegularExpressions.Regex]::Replace($final, '(\r?\n){3,}', "

")

Set-Content -Path "BenhVien.sql" -Value $final -Encoding UTF8
