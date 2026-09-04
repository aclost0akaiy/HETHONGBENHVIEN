$connStr = "Server=DESKTOP-DFA7S8M\SQLEXPRESS;Database=QuanLyBenhVienDb;Integrated Security=True;TrustServerCertificate=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

$sql = @"
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'ActiveIngredient' AND Object_ID = Object_ID(N'Medicines'))
BEGIN
    ALTER TABLE Medicines ADD ActiveIngredient NVARCHAR(200) NULL;
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'Dosage' AND Object_ID = Object_ID(N'Medicines'))
BEGIN
    ALTER TABLE Medicines ADD Dosage NVARCHAR(100) NULL;
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'DosageForm' AND Object_ID = Object_ID(N'Medicines'))
BEGIN
    ALTER TABLE Medicines ADD DosageForm NVARCHAR(100) NULL;
END
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'BatchNumber' AND Object_ID = Object_ID(N'Medicines'))
BEGIN
    ALTER TABLE Medicines ADD BatchNumber NVARCHAR(50) NULL;
END

-- Update NULL fields in existing rows to empty strings
UPDATE Medicines SET ActiveIngredient = N'' WHERE ActiveIngredient IS NULL;
UPDATE Medicines SET Dosage = N'' WHERE Dosage IS NULL;
UPDATE Medicines SET DosageForm = N'' WHERE DosageForm IS NULL;
UPDATE Medicines SET BatchNumber = N'' WHERE BatchNumber IS NULL;
UPDATE Medicines SET Unit = N'' WHERE Unit IS NULL;
UPDATE Medicines SET Category = N'' WHERE Category IS NULL;
UPDATE Medicines SET Manufacturer = N'' WHERE Manufacturer IS NULL;

-- Remove consumables
DELETE FROM Medicines WHERE Category LIKE N'%Vật tư%' OR Category LIKE N'%tiêu hao%' OR Name LIKE N'%Bơm%' OR Name LIKE N'%bông%';

-- Fix unrealistic prices in Medicines table
UPDATE Medicines SET Price = 2500 WHERE Name LIKE N'%Paracetamol%' AND Price > 10000;
UPDATE Medicines SET Price = 3000 WHERE Name LIKE N'%Panadol%' AND Price > 10000 AND Unit = N'Viên';
UPDATE Medicines SET Price = 4000 WHERE Name LIKE N'%Efferalgan%' AND Price > 10000 AND Unit = N'Viên';
UPDATE Medicines SET Price = 8000 WHERE Name LIKE N'%Omeprazol%' OR Name LIKE N'%Omeprazole%';
UPDATE Medicines SET Price = 15000 WHERE Name LIKE N'%Cefuroxime%' OR Name LIKE N'%Clavamox%';
UPDATE Medicines SET Price = 18000 WHERE Name LIKE N'%Ciprobay%' OR Name LIKE N'%Ciprofloxacin%';
UPDATE Medicines SET Price = 5000 WHERE Name LIKE N'%Atropin%';
UPDATE Medicines SET Price = 12000 WHERE Name LIKE N'%Vinphyton%' OR Name LIKE N'%Vitamin K1%';
UPDATE Medicines SET Price = 1500 WHERE Name LIKE N'%Nước cất%';

-- Fix unrealistic prices in PrescriptionDetails table
UPDATE PrescriptionDetails SET Price = 2500 WHERE MedicineName LIKE N'%Paracetamol%' OR MedicineName LIKE N'%Panadol%' OR MedicineName LIKE N'%Hapacol%';
UPDATE PrescriptionDetails SET Price = 8000 WHERE MedicineName LIKE N'%Omeprazol%' OR MedicineName LIKE N'%Omeprazole%';
UPDATE PrescriptionDetails SET Price = 15000 WHERE MedicineName LIKE N'%Cefuroxime%' OR MedicineName LIKE N'%Clavamox%' OR MedicineName LIKE N'%Kháng sinh%';
UPDATE PrescriptionDetails SET Price = 5000 WHERE MedicineName LIKE N'%Atropin%';
UPDATE PrescriptionDetails SET Price = 12000 WHERE MedicineName LIKE N'%Vinphyton%';
UPDATE PrescriptionDetails SET Price = 1500 WHERE MedicineName LIKE N'%Nước cất%';
"@

$cmd = $conn.CreateCommand()
$cmd.CommandText = $sql
$cmd.ExecuteNonQuery()
$conn.Close()
Write-Host "Updated schema & prices in SQL Server successfully!"
