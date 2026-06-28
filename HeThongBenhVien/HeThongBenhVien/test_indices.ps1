$projectRoot = "c:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\HeThongBenhVien"
$viewPath = Join-Path $projectRoot "Views\Doctor\Dashboard.cshtml"

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

Write-Output ("Start index: " + $startIdx)
Write-Output ("End index: " + $endIdx)
if ($startIdx -ne -1 -and $endIdx -ne -1) {
    Write-Output ("Start line content: " + $lines[$startIdx])
    Write-Output ("End line content: " + $lines[$endIdx])
}
