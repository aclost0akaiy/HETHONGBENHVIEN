$connStr = "Server=DESKTOP-DFA7S8M\SQLEXPRESS;Database=QuanLyBenhVienDb;Integrated Security=True;TrustServerCertificate=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

# Delete bogus test rows
$cmdDel = $conn.CreateCommand()
$cmdDel.CommandText = "DELETE FROM Medicines WHERE Name = '1' OR ActiveIngredient = '111' OR Manufacturer = '1'"
$cmdDel.ExecuteNonQuery()

# Parameterized insertions to avoid any encoding / font issues
function Add-Medicine {
    param($Name, $ActiveIngredient, $Dosage, $DosageForm, $BatchNumber, $Price, $Unit, $Category, $StockQuantity, $MinStock, $Manufacturer, $ExpiryDate)
    
    $checkCmd = $conn.CreateCommand()
    $checkCmd.CommandText = "SELECT COUNT(*) FROM Medicines WHERE Name = @Name AND BatchNumber = @BatchNumber"
    $checkCmd.Parameters.AddWithValue("@Name", $Name) | Out-Null
    $checkCmd.Parameters.AddWithValue("@BatchNumber", $BatchNumber) | Out-Null
    $count = [int]$checkCmd.ExecuteScalar()

    if ($count -eq 0) {
        $insCmd = $conn.CreateCommand()
        $insCmd.CommandText = "INSERT INTO Medicines (Name, ActiveIngredient, Dosage, DosageForm, BatchNumber, Price, PurchasePrice, Unit, Category, StockQuantity, MinStock, Manufacturer, ExpiryDate, IsActive) VALUES (@Name, @ActiveIngredient, @Dosage, @DosageForm, @BatchNumber, @Price, @PurchasePrice, @Unit, @Category, @StockQuantity, @MinStock, @Manufacturer, @ExpiryDate, 1)"
        $insCmd.Parameters.AddWithValue("@Name", $Name) | Out-Null
        $insCmd.Parameters.AddWithValue("@ActiveIngredient", $ActiveIngredient) | Out-Null
        $insCmd.Parameters.AddWithValue("@Dosage", $Dosage) | Out-Null
        $insCmd.Parameters.AddWithValue("@DosageForm", $DosageForm) | Out-Null
        $insCmd.Parameters.AddWithValue("@BatchNumber", $BatchNumber) | Out-Null
        $insCmd.Parameters.AddWithValue("@Price", $Price) | Out-Null
        $insCmd.Parameters.AddWithValue("@PurchasePrice", ($Price * 0.7)) | Out-Null
        $insCmd.Parameters.AddWithValue("@Unit", $Unit) | Out-Null
        $insCmd.Parameters.AddWithValue("@Category", $Category) | Out-Null
        $insCmd.Parameters.AddWithValue("@StockQuantity", $StockQuantity) | Out-Null
        $insCmd.Parameters.AddWithValue("@MinStock", $MinStock) | Out-Null
        $insCmd.Parameters.AddWithValue("@Manufacturer", $Manufacturer) | Out-Null
        $insCmd.Parameters.AddWithValue("@ExpiryDate", [DateTime]::Parse($ExpiryDate)) | Out-Null
        $insCmd.ExecuteNonQuery()
    }
}

Add-Medicine "Vitamin C 1000mg" "Acid Ascorbic (Vitamin C)" "1000mg" "Viên sủi" "L2026-VITC" 85000 "Lọ" "Vitamin" 50 10 "Dược Hậu Giang (DHG)" "2028-10-31 23:59:59"
Add-Medicine "Amlodipine 5mg" "Amlodipine besylate" "5mg" "Viên nén" "L2026-AML" 60000 "Viên" "Tim mạch" 200 30 "Stada Vietnam" "2027-08-31 23:59:59"
Add-Medicine "Omeprazole 20mg" "Omeprazole" "20mg" "Viên nang" "L2026-OMP" 8000 "Viên" "Tiêu hóa" 150 20 "Mekophar" "2027-11-30 23:59:59"
Add-Medicine "Cefuroxime 500mg" "Cefuroxime axetil" "500mg" "Viên nén bao phim" "L2026-CEF" 15000 "Viên" "Kháng sinh" 80 100 "Imexpharm" "2027-09-30 23:59:59"
Add-Medicine "Paracetamol 500mg" "Paracetamol" "500mg" "Viên nén" "L2026-B02" 2500 "Viên" "Giảm đau" 100 20 "Dược Hậu Giang" "2026-10-31 23:59:59"

$conn.Close()
Write-Host "Inserted SQL Medicines cleanly without encoding errors!"
