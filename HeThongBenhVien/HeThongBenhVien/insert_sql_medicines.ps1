$sql = @"
USE QuanLyBenhVienDb;

-- Clean up bogus row 29 if exists
DELETE FROM Medicines WHERE Name = N'Thuốc hạ sốt' AND ActiveIngredient = N'111';

-- Insert standard Medicines into SQL Server DB if they do not exist
IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name LIKE N'%Vitamin C%')
BEGIN
    INSERT INTO Medicines (Name, ActiveIngredient, Dosage, DosageForm, BatchNumber, Price, Unit, Category, StockQuantity, MinStock, Manufacturer, ExpiryDate, IsActive)
    VALUES (N'Vitamin C 1000mg', N'Acid Ascorbic (Vitamin C)', N'1000mg', N'Lọ', N'L2026-VITC', 85000, N'Lọ', N'Vitamin', 50, 10, N'Dược Hậu Giang (DHG)', '2028-10-31 23:59:59', 1);
END;

IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name LIKE N'%Amlodipine%')
BEGIN
    INSERT INTO Medicines (Name, ActiveIngredient, Dosage, DosageForm, BatchNumber, Price, Unit, Category, StockQuantity, MinStock, Manufacturer, ExpiryDate, IsActive)
    VALUES (N'Amlodipine 5mg', N'Amlodipine besylate', N'5mg', N'Viên nén', N'L2026-AML', 60000, N'Viên', N'Tim mạch', 200, 30, N'Stada Vietnam', '2027-08-31 23:59:59', 1);
END;

IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name LIKE N'%Omeprazole%')
BEGIN
    INSERT INTO Medicines (Name, ActiveIngredient, Dosage, DosageForm, BatchNumber, Price, Unit, Category, StockQuantity, MinStock, Manufacturer, ExpiryDate, IsActive)
    VALUES (N'Omeprazole 20mg', N'Omeprazole', N'20mg', N'Viên nang', N'L2026-OMP', 8000, N'Viên', N'Tiêu hóa', 150, 20, N'Mekophar', '2027-11-30 23:59:59', 1);
END;

IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name LIKE N'%Cefuroxime%')
BEGIN
    INSERT INTO Medicines (Name, ActiveIngredient, Dosage, DosageForm, BatchNumber, Price, Unit, Category, StockQuantity, MinStock, Manufacturer, ExpiryDate, IsActive)
    VALUES (N'Cefuroxime 500mg', N'Cefuroxime axetil', N'500mg', N'Viên nén bao phim', N'L2026-CEF', 15000, N'Viên', N'Kháng sinh', 80, 100, N'Imexpharm', '2027-09-30 23:59:59', 1);
END;

IF NOT EXISTS (SELECT 1 FROM Medicines WHERE Name LIKE N'%Paracetamol%' AND BatchNumber = N'L2026-B02')
BEGIN
    INSERT INTO Medicines (Name, ActiveIngredient, Dosage, DosageForm, BatchNumber, Price, Unit, Category, StockQuantity, MinStock, Manufacturer, ExpiryDate, IsActive)
    VALUES (N'Paracetamol 500mg', N'Paracetamol', N'500mg', N'Viên nén', N'L2026-B02', 2500, N'Viên', N'Giảm đau', 100, 20, N'Dược Hậu Giang', '2026-10-31 23:59:59', 1);
END;
"@

Invoke-Sqlcmd -ServerInstance 'DESKTOP-DFA7S8M\SQLEXPRESS' -Database 'QuanLyBenhVienDb' -Query $sql
Write-Host "Inserted SQL Medicines successfully!"
