# Define paths to SQL files
$outerSqlPath = "C:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\BenhVien.sql"
$nestedSqlPath = "C:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\HeThongBenhVien\BenhVien.sql"

# 100 Existing medicines with proper Vietnamese diacritics
$existingMedicines = @(
    "Paracetamol 500mg", "Amoxicillin 500mg", "Vitamin C 1000mg", "Omeprazole 20mg", "Ibuprofen 400mg",
    "Cefuroxime 500mg", "Azithromycin 500mg", "Metformin 850mg", "Amlodipine 5mg", "Atorvastatin 10mg",
    "Loratadine 10mg", "Cetirizine 10mg", "Salbutamol 2mg", "Metoprolol 50mg", "Esomeprazole 40mg",
    "Gliclazide 60mg", "Losartan 50mg", "Prednisolone 5mg", "Methylprednisolone 16mg", "Clopidogrel 75mg",
    "Pantoprazole 40mg", "Singulair 10mg", "Telfast 180mg", "Panadol Extra", "Decolgen Forte",
    "Alaxan", "Strepsils Orange", "Maalox", "Smecta", "Enterogermina",
    "Bisolvon 8mg", "Eugica Green", "Ameflu", "Hapacol 250", "Efferalgan 500mg",
    "Gaviscon Dual Action", "Loperamide 2mg", "Smectago", "Berberin 50mg", "Duo-Plavin 75mg/100mg",
    "Co-Diovan 80/12.5mg", "Concor 5mg", "Vastarel MR 35mg", "Diamicron MR 60mg", "Glucophage 500mg",
    "Jardiance 10mg", "Lipitor 20mg", "Crestor 10mg", "Fenofibrate 160mg", "Zyloric 300mg",
    "Colchicine 1mg", "Voltaren 75mg", "Mobic 15mg", "Celebrex 200mg", "Arcoxia 90mg",
    "Nurofen Kids", "Hapacol 650", "Klamentin 875/125", "Zinnat 500mg", "Clarithromycin 500mg",
    "Ciprofloxacin 500mg", "Augmentin 1g", "Bactrim", "Salbubronch", "Symbicort Turbuhaler",
    "Seretide Evohaler", "Aerius 5mg", "Claritin 10mg", "Xyzal 5mg", "Otrivin 0.1%",
    "Naphazolin 0.05%", "Systane Ultra", "Viroto", "Tobrex 0.3%", "Bepanthen",
    "Skinoren Cream", "Gentinic", "Dermovate", "Calamine", "Eumovate",
    "Neurobion", "Tanakan 40mg", "Ginkgo Biloba", "Nootropil 800mg", "Magne B6",
    "Calcium Carbonate", "Enervon", "Obimin", "Decumar", "Gliclada 30mg",
    "Galvus Met", "Glucovance", "Mixtard 30", "Lantus Solostar", "Plavix 75mg",
    "Cardilopin 5mg", "Micardis 40mg", "Coveram 5mg/5mg", "Imdur 60mg", "Dorithricin"
)

# Build a lookup set to keep names unique
$usedNames = New-Object System.Collections.Generic.HashSet[string]
foreach ($med in $existingMedicines) {
    [void]$usedNames.Add($med)
}

# Arrays of ingredients mapped to category
$ingredients = @(
    @{ Name="Diclofenac"; Cat="Giảm đau, kháng viêm"; Unit="Viên" },
    @{ Name="Meloxicam"; Cat="Giảm đau, kháng viêm"; Unit="Viên" },
    @{ Name="Celecoxib"; Cat="Giảm đau, kháng viêm"; Unit="Viên" },
    @{ Name="Etoricoxib"; Cat="Giảm đau, kháng viêm"; Unit="Viên" },
    @{ Name="Ketoprofen"; Cat="Giảm đau, kháng viêm"; Unit="Viên" },
    @{ Name="Naproxen"; Cat="Giảm đau, kháng viêm"; Unit="Viên" },
    @{ Name="Tramadol"; Cat="Giảm đau, hạ sốt"; Unit="Viên" },
    @{ Name="Codeine"; Cat="Giảm đau, hạ sốt"; Unit="Viên" },
    @{ Name="Aspirin"; Cat="Giảm đau, hạ sốt"; Unit="Viên" },
    @{ Name="Mefenamic Acid"; Cat="Giảm đau, hạ sốt"; Unit="Viên" },
    
    @{ Name="Cefadroxil"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Cefaclor"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Cefpodoxime"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Cefdinir"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Clarithromycin"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Erythromycin"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Levofloxacin"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Moxifloxacin"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Metronidazole"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Doxycycline"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Clindamycin"; Cat="Kháng sinh"; Unit="Hộp" },
    @{ Name="Cefotaxime"; Cat="Kháng sinh"; Unit="Ống" },
    @{ Name="Ceftriaxone"; Cat="Kháng sinh"; Unit="Ống" },
    
    @{ Name="Lansoprazole"; Cat="Tiêu hóa"; Unit="Hộp" },
    @{ Name="Pantoprazole"; Cat="Tiêu hóa"; Unit="Hộp" },
    @{ Name="Rabeprazole"; Cat="Tiêu hóa"; Unit="Hộp" },
    @{ Name="Esomeprazole"; Cat="Tiêu hóa"; Unit="Hộp" },
    @{ Name="Domperidone"; Cat="Tiêu hóa"; Unit="Viên" },
    @{ Name="Metoclopramide"; Cat="Tiêu hóa"; Unit="Viên" },
    @{ Name="Simethicone"; Cat="Tiêu hóa"; Unit="Hộp" },
    @{ Name="Trimebutine"; Cat="Tiêu hóa"; Unit="Hộp" },
    @{ Name="Phosphalugel"; Cat="Tiêu hóa"; Unit="Hộp" },
    
    @{ Name="Nifedipine"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Felodipine"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Lisinopril"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Enalapril"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Captopril"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Valsartan"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Irbesartan"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Telmisartan"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Bisoprolol"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Propranolol"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Carvedilol"; Cat="Tim mạch"; Unit="Hộp" },
    @{ Name="Spironolactone"; Cat="Tim mạch"; Unit="Hộp" },
    
    @{ Name="Rosuvastatin"; Cat="Mỡ máu"; Unit="Hộp" },
    @{ Name="Simvastatin"; Cat="Mỡ máu"; Unit="Hộp" },
    @{ Name="Gemfibrozil"; Cat="Mỡ máu"; Unit="Hộp" },
    
    @{ Name="Levocetirizine"; Cat="Dị ứng"; Unit="Hộp" },
    @{ Name="Chlorpheniramine"; Cat="Dị ứng"; Unit="Hộp" },
    @{ Name="Terbutaline"; Cat="Hô hấp"; Unit="Hộp" },
    @{ Name="Theophylline"; Cat="Hô hấp"; Unit="Hộp" },
    @{ Name="Budesonide"; Cat="Hô hấp"; Unit="Lọ" },
    @{ Name="Fluticasone"; Cat="Hô hấp"; Unit="Lọ" },
    @{ Name="Montelukast"; Cat="Hô hấp"; Unit="Hộp" },
    @{ Name="Bromhexine"; Cat="Hô hấp"; Unit="Hộp" },
    @{ Name="Ambroxol"; Cat="Hô hấp"; Unit="Hộp" },
    @{ Name="Acetylcysteine"; Cat="Hô hấp"; Unit="Hộp" },
    @{ Name="Dextromethorphan"; Cat="Hô hấp"; Unit="Hộp" },
    
    @{ Name="Glimepiride"; Cat="Tiểu đường"; Unit="Hộp" },
    @{ Name="Glibenclamide"; Cat="Tiểu đường"; Unit="Hộp" },
    @{ Name="Pioglitazone"; Cat="Tiểu đường"; Unit="Hộp" },
    @{ Name="Sitagliptin"; Cat="Tiểu đường"; Unit="Hộp" },
    @{ Name="Vildagliptin"; Cat="Tiểu đường"; Unit="Hộp" },
    @{ Name="Linagliptin"; Cat="Tiểu đường"; Unit="Hộp" },
    @{ Name="Dapagliflozin"; Cat="Tiểu đường"; Unit="Hộp" },
    
    @{ Name="Vitamin A"; Cat="Vitamin & Khoáng chất"; Unit="Lọ" },
    @{ Name="Vitamin B1"; Cat="Vitamin & Khoáng chất"; Unit="Lọ" },
    @{ Name="Vitamin B6"; Cat="Vitamin & Khoáng chất"; Unit="Lọ" },
    @{ Name="Vitamin B12"; Cat="Vitamin & Khoáng chất"; Unit="Ống" },
    @{ Name="Vitamin E"; Cat="Vitamin & Khoáng chất"; Unit="Lọ" },
    @{ Name="Zinc Sulfate"; Cat="Vitamin & Khoáng chất"; Unit="Hộp" },
    @{ Name="Ferrous Sulfate"; Cat="Vitamin & Khoáng chất"; Unit="Hộp" },
    @{ Name="Folic Acid"; Cat="Vitamin & Khoáng chất"; Unit="Hộp" },
    
    @{ Name="Aciclovir"; Cat="Da liễu"; Unit="Tuýp" },
    @{ Name="Ketoconazole"; Cat="Da liễu"; Unit="Tuýp" },
    @{ Name="Fluconazole"; Cat="Da liễu"; Unit="Hộp" },
    @{ Name="Itraconazole"; Cat="Da liễu"; Unit="Hộp" },
    @{ Name="Hydrocortisone"; Cat="Da liễu"; Unit="Tuýp" },
    @{ Name="Betamethasone"; Cat="Da liễu"; Unit="Tuýp" }
)

$dosages = @("2.5mg", "5mg", "10mg", "15mg", "20mg", "30mg", "40mg", "50mg", "75mg", "80mg", "100mg", "120mg", "150mg", "200mg", "250mg", "300mg", "400mg", "500mg", "625mg", "850mg", "1g")
$manufacturers = @("Dược Hậu Giang", "Domesco", "Traphaco", "Hasan", "Stella", "Boston", "OPV", "OPC", "Sanofi", "AstraZeneca", "GSK", "Pfizer", "Novartis", "Roche", "MSD", "Abbott", "Bayer", "Boehringer Ingelheim")

# Generate 900 new unique medicines
$newMedicinesList = New-Object System.Collections.Generic.List[string]
$rand = New-Object System.Random
$count = 0

while ($count -lt 900) {
    $ing = $ingredients[$rand.Next($ingredients.Length)]
    $dos = $dosages[$rand.Next($dosages.Length)]
    $name = "$($ing.Name) $dos"
    
    if (-not $usedNames.Contains($name)) {
        [void]$usedNames.Add($name)
        
        $price = $rand.Next(5, 80) * 10000
        $unit = $ing.Unit
        $category = $ing.Cat
        $stock = $rand.Next(10, 250) * 10
        $minStock = $rand.Next(10, 100)
        $manufacturer = $manufacturers[$rand.Next($manufacturers.Length)]
        
        $dateType = $rand.Next(1, 11)
        $expiryDateStr = "NULL"
        if ($dateType -eq 1) {
            $expiryDateStr = "'" + (Get-Date).AddDays(-$rand.Next(10, 300)).ToString("yyyy-MM-dd") + "'"
        } elseif ($dateType -eq 2) {
            $expiryDateStr = "'" + (Get-Date).AddDays($rand.Next(1, 29)).ToString("yyyy-MM-dd") + "'"
        } elseif ($dateType -eq 3) {
            $expiryDateStr = "'" + (Get-Date).AddDays($rand.Next(31, 89)).ToString("yyyy-MM-dd") + "'"
        } else {
            $expiryDateStr = "'" + (Get-Date).AddDays($rand.Next(365, 1000)).ToString("yyyy-MM-dd") + "'"
        }
        
        $sqlLine = "(N'$name', $price, N'$unit', N'$category', $stock, $minStock, N'$manufacturer', $expiryDateStr, 1)"
        $newMedicinesList.Add($sqlLine)
        $count++
    }
}

# 100 original medicines values formatted with proper accents
$fullSqlValues = New-Object System.Collections.Generic.List[string]
$originalSqlLines = @(
    "(N'Paracetamol 500mg', 50000, N'Hộp', N'Giảm đau, hạ sốt', 110, 20, N'Dược Hậu Giang', '2025-12-31', 1)",
    "(N'Amoxicillin 500mg', 120000, N'Hộp', N'Kháng sinh', 50, 10, N'Domesco', '2026-06-30', 1)",
    "(N'Vitamin C 1000mg', 85000, N'Lọ', N'Vitamin & Khoáng chất', 200, 30, N'Traphaco', '2027-01-15', 1)",
    "(N'Omeprazole 20mg', 150000, N'Hộp', N'Tiêu hóa', 80, 15, N'Hasan', '2025-10-20', 1)",
    "(N'Ibuprofen 400mg', 75000, N'Hộp', N'Giảm đau, kháng viêm', 120, 25, N'Stella', '2026-03-10', 1)",
    "(N'Cefuroxime 500mg', 150000, N'Hộp', N'Kháng sinh', 90, 20, N'Dược Hậu Giang', '2026-08-05', 1)",
    "(N'Azithromycin 500mg', 185000, N'Hộp', N'Kháng sinh', 150, 20, N'Stella', '2026-09-20', 1)",
    "(N'Metformin 850mg', 95000, N'Hộp', N'Tiểu đường', 1200, 50, N'Boston', '2027-05-18', 1)",
    "(N'Amlodipine 5mg', 60000, N'Hộp', N'Tim mạch', 1500, 40, N'Hasan', '2026-08-10', 1)",
    "(N'Atorvastatin 10mg', 130000, N'Hộp', N'Mỡ máu', 850, 30, N'Stella', '2027-11-12', 1)",
    "(N'Loratadine 10mg', 45000, N'Hộp', N'Dị ứng', 60, 15, N'Traphaco', '2026-02-15', 1)",
    "(N'Cetirizine 10mg', 48000, N'Hộp', N'Dị ứng', 95, 15, N'Hasan', '2026-08-15', 1)",
    "(N'Salbutamol 2mg', 55000, N'Hộp', N'Hô hấp', 180, 20, N'Dược Hậu Giang', '2026-10-10', 1)",
    "(N'Metoprolol 50mg', 115000, N'Hộp', N'Tim mạch', 1100, 30, N'Boston', '2027-02-28', 1)",
    "(N'Esomeprazole 40mg', 220000, N'Hộp', N'Tiêu hóa', 130, 25, N'Stella', '2026-09-05', 1)",
    "(N'Gliclazide 60mg', 140000, N'Hộp', N'Tiểu đường', 950, 30, N'Hasan', '2027-08-25', 1)",
    "(N'Losartan 50mg', 125000, N'Hộp', N'Tim mạch', 720, 25, N'Dược Hậu Giang', '2026-08-22', 1)",
    "(N'Prednisolone 5mg', 50000, N'Hộp', N'Kháng viêm', 40, 10, N'Stella', '2026-04-20', 1)",
    "(N'Methylprednisolone 16mg', 160000, N'Hộp', N'Kháng viêm', 1050, 40, N'Boston', '2027-03-15', 1)",
    "(N'Clopidogrel 75mg', 210000, N'Hộp', N'Tim mạch', 140, 20, N'Hasan', '2026-09-12', 1)",
    "(N'Pantoprazole 40mg', 180000, N'Hộp', N'Tiêu hóa', 250, 30, N'Dược Hậu Giang', '2027-04-30', 1)",
    "(N'Singulair 10mg', 350000, N'Hộp', N'Hô hấp', 80, 15, N'MSD', '2026-07-28', 1)",
    "(N'Telfast 180mg', 190000, N'Hộp', N'Dị ứng', 120, 20, N'Sanofi', '2027-06-15', 1)",
    "(N'Panadol Extra', 85000, N'Hộp', N'Giảm đau, hạ sốt', 2500, 100, N'GSK', '2028-02-28', 1)",
    "(N'Decolgen Forte', 42000, N'Hộp', N'Trị cảm cúm', 1800, 50, N'United Pharma', '2026-09-08', 1)",
    "(N'Alaxan', 98000, N'Hộp', N'Giảm đau, kháng viêm', 70, 20, N'United Pharma', '2025-11-30', 1)",
    "(N'Strepsils Orange', 35000, N'Hộp', N'Trị đau họng', 350, 30, N'Reckitt', '2027-09-15', 1)",
    "(N'Maalox', 65000, N'Hộp', N'Tiêu hóa', 400, 25, N'Sanofi', '2026-08-01', 1)",
    "(N'Smecta', 110000, N'Hộp', N'Tiêu hóa', 500, 30, N'Ipsen', '2027-12-10', 1)",
    "(N'Enterogermina', 280000, N'Hộp', N'Men tiêu hóa', 300, 20, N'Sanofi', '2026-09-30', 1)",
    "(N'Bisolvon 8mg', 72000, N'Hộp', N'Hô hấp', 150, 15, N'Boehringer Ingelheim', '2026-03-25', 1)",
    "(N'Eugica Green', 55000, N'Hộp', N'Hô hấp', 600, 40, N'Mega We Care', '2026-10-05', 1)",
    "(N'Ameflu', 45000, N'Hộp', N'Trị cảm cúm', 1200, 50, N'OPV', '2027-10-20', 1)",
    "(N'Hapacol 250', 38000, N'Hộp', N'Hạ sốt trẻ em', 85, 20, N'Dược Hậu Giang', '2026-05-15', 1)",
    "(N'Efferalgan 500mg', 70000, N'Hộp', N'Giảm đau, hạ sốt', 2200, 100, N'Bristol-Myers Squibb', '2028-05-12', 1)",
    "(N'Gaviscon Dual Action', 160000, N'Hộp', N'Tiêu hóa', 450, 30, N'Reckitt', '2026-08-25', 1)",
    "(N'Loperamide 2mg', 30000, N'Hộp', N'Tiêu hóa', 90, 15, N'Stella', '2025-09-12', 1)",
    "(N'Smectago', 135000, N'Hộp', N'Tiêu hóa', 210, 20, N'Ipsen', '2026-10-15', 1)",
    "(N'Berberin 50mg', 15000, N'Lọ', N'Tiêu hóa', 3000, 100, N'OPC', '2027-12-30', 1)",
    "(N'Duo-Plavin 75mg/100mg', 650000, N'Hộp', N'Tim mạch', 50, 10, N'Sanofi', '2026-09-02', 1)",
    "(N'Co-Diovan 80/12.5mg', 380000, N'Hộp', N'Tim mạch', 110, 15, N'Novartis', '2026-08-08', 1)",
    "(N'Concor 5mg', 145000, N'Hộp', N'Tim mạch', 880, 30, N'Merck', '2027-04-15', 1)",
    "(N'Vastarel MR 35mg', 185000, N'Hộp', N'Tim mạch', 1020, 40, N'Servier', '2027-08-20', 1)",
    "(N'Diamicron MR 60mg', 250000, N'Hộp', N'Tiểu đường', 940, 30, N'Servier', '2026-10-25', 1)",
    "(N'Glucophage 500mg', 80000, N'Hộp', N'Tiểu đường', 1300, 50, N'Merck', '2027-02-10', 1)",
    "(N'Jardiance 10mg', 850000, N'Hộp', N'Tiểu đường', 40, 10, N'Boehringer Ingelheim', '2026-08-18', 1)",
    "(N'Lipitor 20mg', 490000, N'Hộp', N'Mỡ máu', 120, 15, N'Pfizer', '2026-09-15', 1)",
    "(N'Crestor 10mg', 420000, N'Hộp', N'Mỡ máu', 180, 20, N'AstraZeneca', '2026-08-30', 1)",
    "(N'Fenofibrate 160mg', 110000, N'Hộp', N'Mỡ máu', 700, 30, N'Hasan', '2027-01-20', 1)",
    "(N'Zyloric 300mg', 160000, N'Hộp', N'Trị Gút', 65, 10, N'Aspen', '2026-01-10', 1)",
    "(N'Colchicine 1mg', 120000, N'Hộp', N'Trị Gút', 150, 15, N'Stella', '2026-10-12', 1)",
    "(N'Voltaren 75mg', 35000, N'Ống', N'Giảm đau, kháng viêm', 500, 50, N'Novartis', '2026-08-04', 1)",
    "(N'Mobic 15mg', 260000, N'Hộp', N'Giảm đau, kháng viêm', 80, 15, N'Boehringer Ingelheim', '2026-04-10', 1)",
    "(N'Celebrex 200mg', 390000, N'Hộp', N'Giảm đau, kháng viêm', 350, 20, N'Pfizer', '2027-05-15', 1)",
    "(N'Arcoxia 90mg', 290000, N'Hộp', N'Giảm đau, kháng viêm', 220, 20, N'MSD', '2026-09-18', 1)",
    "(N'Nurofen Kids', 125000, N'Chai', N'Giảm đau hạ sốt', 110, 15, N'Reckitt', '2026-08-12', 1)",
    "(N'Hapacol 650', 65000, N'Hộp', N'Giảm đau, hạ sốt', 1600, 50, N'DHG', '2027-11-30', 1)",
    "(N'Klamentin 875/125', 190000, N'Hộp', N'Kháng sinh', 190, 20, N'DHG', '2026-10-20', 1)",
    "(N'Zinnat 500mg', 290000, N'Hộp', N'Kháng sinh', 280, 20, N'GSK', '2026-08-28', 1)",
    "(N'Clarithromycin 500mg', 175000, N'Hộp', N'Kháng sinh', 140, 15, N'Stella', '2027-03-20', 1)",
    "(N'Ciprofloxacin 500mg', 95000, N'Hộp', N'Kháng sinh', 85, 15, N'Boston', '2025-12-15', 1)",
    "(N'Augmentin 1g', 350000, N'Hộp', N'Kháng sinh', 320, 25, N'GSK', '2027-09-08', 1)",
    "(N'Bactrim', 80000, N'Hộp', N'Kháng sinh', 130, 15, N'Roche', '2026-09-09', 1)",
    "(N'Salbubronch', 85000, N'Chai', N'Hô hấp', 90, 15, N'Stella', '2026-02-28', 1)",
    "(N'Symbicort Turbuhaler', 620000, N'Lọ', N'Hô hấp', 60, 10, N'AstraZeneca', '2026-08-14', 1)",
    "(N'Seretide Evohaler', 420000, N'Lọ', N'Hô hấp', 75, 10, N'GSK', '2026-10-02', 1)",
    "(N'Aerius 5mg', 125000, N'Hộp', N'Dị ứng', 420, 30, N'MSD', '2027-04-18', 1)",
    "(N'Claritin 10mg', 115000, N'Hộp', N'Dị ứng', 310, 25, N'Bayer', '2026-09-22', 1)",
    "(N'Xyzal 5mg', 140000, N'Hộp', N'Dị ứng', 55, 10, N'UCB', '2026-03-30', 1)",
    "(N'Otrivin 0.1%', 55000, N'Lọ', N'Nhỏ mũi', 800, 30, N'Novartis', '2026-08-09', 1)",
    "(N'Naphazolin 0.05%', 8000, N'Lọ', N'Nhỏ mũi', 1500, 50, N'DHG', '2027-08-10', 1)",
    "(N'Systane Ultra', 95000, N'Lọ', N'Nhỏ mắt', 1100, 40, N'Alcon', '2027-02-15', 1)",
    "(N'Viroto', 45000, N'Lọ', N'Nhỏ mắt', 900, 30, N'Rohto', '2026-09-10', 1)",
    "(N'Tobrex 0.3%', 68000, N'Lọ', N'Nhỏ mắt', 400, 20, N'Alcon', '2026-08-20', 1)",
    "(N'Bepanthen', 85000, N'Tuýp', N'Da liễu', 650, 30, N'Bayer', '2027-05-20', 1)",
    "(N'Skinoren Cream', 320000, N'Tuýp', N'Da liễu', 120, 15, N'Leo Pharma', '2026-09-25', 1)",
    "(N'Gentinic', 25000, N'Tuýp', N'Da liễu', 80, 15, N'Shinpoong', '2025-10-15', 1)",
    "(N'Dermovate', 95000, N'Tuýp', N'Da liễu', 350, 20, N'GSK', '2026-08-03', 1)",
    "(N'Calamine', 65000, N'Chai', N'Da liễu', 240, 20, N'Stella', '2027-03-12', 1)",
    "(N'Eumovate', 75000, N'Tuýp', N'Da liễu', 410, 25, N'GSK', '2027-08-15', 1)",
    "(N'Neurobion', 110000, N'Hộp', N'Thần kinh', 1050, 40, N'Merck', '2027-06-30', 1)",
    "(N'Tanakan 40mg', 280000, N'Hộp', N'Tuần hoàn não', 95, 15, N'Ipsen', '2026-03-15', 1)",
    "(N'Ginkgo Biloba', 150000, N'Hộp', N'Tuần hoàn não', 1800, 50, N'Mason', '2027-12-18', 1)",
    "(N'Nootropil 800mg', 210000, N'Hộp', N'Thần kinh', 140, 15, N'UCB', '2026-09-14', 1)",
    "(N'Magne B6', 95000, N'Hộp', N'Vitamin & Khoáng chất', 2200, 100, N'Sanofi', '2027-10-15', 1)",
    "(N'Calcium Carbonate', 160000, N'Hộp', N'Vitamin & Khoáng chất', 1400, 50, N'Sanofi', '2027-04-20', 1)",
    "(N'Enervon', 72000, N'Hộp', N'Vitamin & Khoáng chất', 900, 45, N'United Pharma', '2026-08-07', 1)",
    "(N'Obimin', 195000, N'Hộp', N'Vitamin & Khoáng chất', 500, 25, N'United Pharma', '2026-10-22', 1)",
    "(N'Decumar', 75000, N'Tuýp', N'Da liễu', 380, 20, N'CVI', '2027-09-30', 1)",
    "(N'Gliclada 30mg', 115000, N'Hộp', N'Tiểu đường', 720, 25, N'Krka', '2026-09-11', 1)",
    "(N'Galvus Met', 450000, N'Hộp', N'Tiểu đường', 110, 15, N'Novartis', '2026-08-21', 1)",
    "(N'Glucovance', 210000, N'Hộp', N'Tiểu đường', 350, 20, N'Merck', '2027-03-25', 1)",
    "(N'Mixtard 30', 185000, N'Lọ', N'Tiểu đường', 90, 10, N'Novo Nordisk', '2026-02-10', 1)",
    "(N'Lantus Solostar', 420000, N'Lọ', N'Tiểu đường', 120, 15, N'Sanofi', '2026-08-30', 1)",
    "(N'Plavix 75mg', 550000, N'Hộp', N'Tim mạch', 150, 15, N'Sanofi', '2027-01-30', 1)",
    "(N'Cardilopin 5mg', 95000, N'Hộp', N'Tim mạch', 80, 10, N'Egis', '2026-01-20', 1)",
    "(N'Micardis 40mg', 310000, N'Hộp', N'Tim mạch', 240, 15, N'Boehringer Ingelheim', '2026-09-08', 1)",
    "(N'Coveram 5mg/5mg', 360000, N'Hộp', N'Tim mạch', 310, 20, N'Servier', '2027-05-15', 1)",
    "(N'Imdur 60mg', 260000, N'Hộp', N'Tim mạch', 150, 15, N'AstraZeneca', '2026-08-11', 1)",
    "(N'Dorithricin', 85000, N'Hộp', N'Trị viêm họng', 600, 30, N'Medice', '2026-10-18', 1)"
)
foreach ($line in $originalSqlLines) {
    $fullSqlValues.Add($line)
}
foreach ($line in $newMedicinesList) {
    $fullSqlValues.Add($line)
}

$sqlInsertStatement = "INSERT INTO Medicines (Name, Price, Unit, Category, StockQuantity, MinStock, Manufacturer, ExpiryDate, IsActive) VALUES `r`n" + ($fullSqlValues -join ",`r`n") + ";"

# Function to replace seed section in file line-by-line to avoid regex issues
function Update-SqlFile {
    param ($filePath)
    if (Test-Path $filePath) {
        $lines = [System.IO.File]::ReadAllLines($filePath, [System.Text.Encoding]::UTF8)
        $startIndex = -1
        $endIndex = -1
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -like "*INSERT INTO Medicines*") {
                $startIndex = $i
            }
            if ($startIndex -ne -1 -and $lines[$i] -eq "GO" -and $endIndex -eq -1 -and $i -gt $startIndex) {
                $endIndex = $i
                break
            }
        }
        
        if ($startIndex -ne -1 -and $endIndex -ne -1) {
            $newLines = $lines[0..($startIndex-1)] + $sqlInsertStatement + $lines[$endIndex..($lines.Length-1)]
            $utf8WithBom = New-Object System.Text.UTF8Encoding($true)
            [System.IO.File]::WriteAllLines($filePath, $newLines, $utf8WithBom)
            Write-Host "Updated $filePath with 1000 medicines seed data successfully."
        } else {
            Write-Host "Failed to find seed section in $filePath. Start: $startIndex, End: $endIndex"
        }
    }
}

Update-SqlFile $outerSqlPath
Update-SqlFile $nestedSqlPath

# Update the local database
$connectionString = "Data Source=PC-202212270109\SQLEXPRESS;Initial Catalog=QuanLyBenhVienDb;Integrated Security=True;TrustServerCertificate=True"
$query = "DELETE FROM Medicines;`r`n" + $sqlInsertStatement

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = $query
    $rowsAffected = $command.ExecuteNonQuery()
    Write-Host "Success! Inserted $rowsAffected medicines directly into local database."
    $connection.Close()
} catch {
    Write-Host "Error running database update:"
    Write-Host $_.Exception.Message
}
