$content = Get-Content -Path "BenhVien.sql" -Raw -Encoding UTF8
$idx = $content.IndexOf("IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL")
if ($idx -ne -1) {
    $content = $content.Substring(0, $idx)
}

$content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?is)CREATE TABLE\s+\[?[a-zA-Z0-9_]+\]?\s*\([^;]+;', '')
$content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?i)ALTER TABLE\s+.*?;\s*', '')
$content = [System.Text.RegularExpressions.Regex]::Replace($content, '(?i)IF OBJECT_ID.*?;\s*', '')

Set-Content -Path "Data_Only.sql" -Value $content -Encoding UTF8
