$utf8bom = New-Object System.Text.UTF8Encoding($true)

# Find all .cshtml files in Views
$files = Get-ChildItem -Path "Views" -Filter *.cshtml -Recurse

foreach ($file in $files) {
    $p = $file.FullName
    $bytes = [System.IO.File]::ReadAllBytes($p)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        # Already has UTF-8 BOM, skip
        continue
    }
    
    # Check if there is any non-ASCII character to avoid converting plain ASCII files
    # (though converting them is harmless, keeping them plain is also fine)
    $hasNonAscii = $false
    foreach ($b in $bytes) {
        if ($b -gt 127) {
            $hasNonAscii = $true
            break
        }
    }
    
    if ($hasNonAscii) {
        $content = [System.IO.File]::ReadAllText($p, [System.Text.Encoding]::UTF8)
        [System.IO.File]::WriteAllText($p, $content, $utf8bom)
        Write-Output "Converted to UTF-8 BOM: $($file.Name)"
    }
}

# Also check and convert Controllers and Models .cs files
$csFiles = Get-ChildItem -Path "Controllers", "Models" -Filter *.cs -Recurse -ErrorAction SilentlyContinue

foreach ($file in $csFiles) {
    $p = $file.FullName
    $bytes = [System.IO.File]::ReadAllBytes($p)
    if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
        continue
    }
    
    $hasNonAscii = $false
    foreach ($b in $bytes) {
        if ($b -gt 127) {
            $hasNonAscii = $true
            break
        }
    }
    
    if ($hasNonAscii) {
        $content = [System.IO.File]::ReadAllText($p, [System.Text.Encoding]::UTF8)
        [System.IO.File]::WriteAllText($p, $content, $utf8bom)
        Write-Output "Converted to UTF-8 BOM: $($file.Name)"
    }
}
