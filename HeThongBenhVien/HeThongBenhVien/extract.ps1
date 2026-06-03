$efSchema = Get-Content -Path "ef_schema.sql" -Raw
$benhVien = Get-Content -Path "BenhVien.sql" -Raw

$tables = @("BloodBanks", "MedicalEquipments", "MedicalServices", "QualityReviews", "DiagnosticImages", "InsuranceCards", "Receptions", "Surgeries", "ICD10Protocols")

$appendStr = "`r`n`r`n-- Bảng bổ sung từ Model`r`n"
foreach ($table in $tables) {
    if ($benhVien -notmatch "CREATE TABLE \[$table\]" -and $benhVien -notmatch "CREATE TABLE $table") {
        if ($efSchema -match "(?s)CREATE TABLE \[$table\] \([^;]*\);") {
            $appendStr += $matches[0] + "`r`nGO`r`n`r`n"
        }
    }
}

Add-Content -Path "BenhVien.sql" -Value $appendStr
