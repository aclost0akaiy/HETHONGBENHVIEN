$lines = Get-Content "c:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\HeThongBenhVien\Views\Doctor\DanhSach.cshtml" -Encoding UTF8
$newLines = @()
for ($i=0; $i -lt 123; $i++) {
    $newLines += $lines[$i]
}
$newLines += "                        <button type=`"submit`" class=`"btn btn-primary px-4`" style=`"border-radius: 8px;`"><i class=`"fa-solid fa-save me-2`"></i>Lưu Bệnh Nhân</button>"
$newLines += "                    </div>"
$newLines += "                </form>"
$newLines += "            </div>"
$newLines += "        </div>"
$newLines += "    </div>"
$newLines += "</div>"
$newLines += ""
for ($i=254; $i -lt $lines.Length; $i++) {
    $newLines += $lines[$i]
}
$newLines | Set-Content "c:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\HeThongBenhVien\Views\Doctor\DanhSach.cshtml" -Encoding UTF8
