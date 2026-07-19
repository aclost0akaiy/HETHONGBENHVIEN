$connectionString = "Data Source=PC-202212270109\SQLEXPRESS;Initial Catalog=QuanLyBenhVienDb;Integrated Security=True;TrustServerCertificate=True"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $command = $connection.CreateCommand()
    
    # Active expired count
    $command.CommandText = "SELECT COUNT(*) FROM Medicines WHERE ExpiryDate < GETDATE() AND IsActive = 1"
    $activeExpiredCount = $command.ExecuteScalar()
    
    # Inactive expired count
    $command.CommandText = "SELECT COUNT(*) FROM Medicines WHERE ExpiryDate < GETDATE() AND IsActive = 0"
    $inactiveExpiredCount = $command.ExecuteScalar()
    
    # Nearly out of stock (StockQuantity <= 1000) count
    $command.CommandText = "SELECT COUNT(*) FROM Medicines WHERE StockQuantity <= 1000 AND IsActive = 1"
    $nearlyOutOfStockCount = $command.ExecuteScalar()
    
    # Medicines with StockQuantity >= 2000 count
    $command.CommandText = "SELECT COUNT(*) FROM Medicines WHERE StockQuantity >= 2000"
    $geq2000Count = $command.ExecuteScalar()
    
    Write-Host "Active Expired Count: $activeExpiredCount"
    Write-Host "Inactive Expired Count: $inactiveExpiredCount"
    Write-Host "Nearly Out of Stock Count (<= 1000): $nearlyOutOfStockCount"
    Write-Host "Medicines with Stock >= 2000 Count: $geq2000Count"
    
    $connection.Close()
} catch {
    Write-Host "Error:"
    Write-Host $_.Exception.Message
}
