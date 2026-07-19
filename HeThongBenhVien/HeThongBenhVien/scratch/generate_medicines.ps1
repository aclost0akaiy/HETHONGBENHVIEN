# Define paths to SQL files
$outerSqlPath = "C:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\BenhVien.sql"
$nestedSqlPath = "C:\Users\PC\Downloads\HETHONGBENHVIEN\HeThongBenhVien\HeThongBenhVien\BenhVien.sql"

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

# 100 original medicines list.
# We will define them with at least 2000 quantity.
# We will keep exactly 5 active expired medicines (1), and set all other expired ones to (0).
$originalMeds = @(
    # 5 Active Expired Medicines (IsActive = 1)
    @{ Name="Paracetamol 500mg"; Price=50000; Unit="Hộp"; Cat="Giảm đau, hạ sốt"; Qty=2200; Min=20; Man="Dược Hậu Giang"; Exp="'2025-12-31'"; Act=1 },
    @{ Name="Amoxicillin 500mg"; Price=120000; Unit="Hộp"; Cat="Kháng sinh"; Qty=2150; Min=10; Man="Domesco"; Exp="'2026-06-30'"; Act=1 },
    @{ Name="Omeprazole 20mg"; Price=150000; Unit="Hộp"; Cat="Tiêu hóa"; Qty=2300; Min=15; Man="Hasan"; Exp="'2025-10-20'"; Act=1 },
    @{ Name="Ibuprofen 400mg"; Price=75000; Unit="Hộp"; Cat="Giảm đau, kháng viêm"; Qty=2400; Min=25; Man="Stella"; Exp="'2026-03-10'"; Act=1 },
    @{ Name="Loperamide 2mg"; Price=30000; Unit="Hộp"; Cat="Tiêu hóa"; Qty=2050; Min=15; Man="Stella"; Exp="'2025-09-12'"; Act=1 },

    # Inactive Expired Medicines (IsActive = 0)
    @{ Name="Alaxan"; Price=98000; Unit="Hộp"; Cat="Giảm đau, kháng viêm"; Qty=2100; Min=20; Man="United Pharma"; Exp="'2025-11-30'"; Act=0 },
    @{ Name="Gentinic"; Price=25000; Unit="Tuýp"; Cat="Da liễu"; Qty=2080; Min=15; Man="Shinpoong"; Exp="'2025-10-15'"; Act=0 },
    @{ Name="Zyloric 300mg"; Price=160000; Unit="Hộp"; Cat="Trị Gút"; Qty=2110; Min=10; Man="Aspen"; Exp="'2026-01-10'"; Act=0 },
    @{ Name="Mobic 15mg"; Price=260000; Unit="Hộp"; Cat="Giảm đau, kháng viêm"; Qty=2350; Min=15; Man="Boehringer Ingelheim"; Exp="'2026-04-10'"; Act=0 },
    @{ Name="Ciprofloxacin 500mg"; Price=95000; Unit="Hộp"; Cat="Kháng sinh"; Qty=2120; Min=15; Man="Boston"; Exp="'2025-12-15'"; Act=0 },
    @{ Name="Salbubronch"; Price=85000; Unit="Chai"; Cat="Hô kháp"; Qty=2180; Min=15; Man="Stella"; Exp="'2026-02-28'"; Act=0 },
    @{ Name="Xyzal 5mg"; Price=140000; Unit="Hộp"; Cat="Dị ứng"; Qty=2090; Min=10; Man="UCB"; Exp="'2026-03-30'"; Act=0 },
    @{ Name="Tanakan 40mg"; Price=280000; Unit="Hộp"; Cat="Tuần hoàn não"; Qty=2040; Min=15; Man="Ipsen"; Exp="'2026-03-15'"; Act=0 },
    @{ Name="Mixtard 30"; Price=185000; Unit="Lọ"; Cat="Tiểu đường"; Qty=2030; Min=10; Man="Novo Nordisk"; Exp="'2026-02-10'"; Act=0 },
    @{ Name="Cardilopin 5mg"; Price=95000; Unit="Hộp"; Cat="Tim mạch"; Qty=2140; Min=10; Man="Egis"; Exp="'2026-01-20'"; Act=0 },
    @{ Name="Bisolvon 8mg"; Price=72000; Unit="Hộp"; Cat="Hô hấp"; Qty=2070; Min=15; Man="Boehringer Ingelheim"; Exp="'2026-03-25'"; Act=0 },

    # Active Future/No Expiry Medicines (IsActive = 1)
    @{ Name="Vitamin C 1000mg"; Price=85000; Unit="Lọ"; Cat="Vitamin & Khoáng chất"; Qty=2500; Min=30; Man="Traphaco"; Exp="'2027-01-15'"; Act=1 },
    @{ Name="Cefuroxime 500mg"; Price=150000; Unit="Hộp"; Cat="Kháng sinh"; Qty=2600; Min=20; Man="Dược Hậu Giang"; Exp="'2026-08-05'"; Act=1 },
    @{ Name="Azithromycin 500mg"; Price=185000; Unit="Hộp"; Cat="Kháng sinh"; Qty=2700; Min=20; Man="Stella"; Exp="'2026-09-20'"; Act=1 },
    @{ Name="Metformin 850mg"; Price=95000; Unit="Hộp"; Cat="Tiểu đường"; Qty=3200; Min=50; Man="Boston"; Exp="'2027-05-18'"; Act=1 },
    @{ Name="Amlodipine 5mg"; Price=60000; Unit="Hộp"; Cat="Tim mạch"; Qty=3500; Min=40; Man="Hasan"; Exp="'2026-08-10'"; Act=1 },
    @{ Name="Atorvastatin 10mg"; Price=130000; Unit="Hộp"; Cat="Mỡ máu"; Qty=2850; Min=30; Man="Stella"; Exp="'2027-11-12'"; Act=1 },
    @{ Name="Loratadine 10mg"; Price=45000; Unit="Hộp"; Cat="Dị ứng"; Qty=2060; Min=15; Man="Traphaco"; Exp="'2026-02-15'"; Act=0 }, # Expired, Inactive
    @{ Name="Cetirizine 10mg"; Price=48000; Unit="Hộp"; Cat="Dị ứng"; Qty=2950; Min=15; Man="Hasan"; Exp="'2026-08-15'"; Act=1 },
    @{ Name="Salbutamol 2mg"; Price=55000; Unit="Hộp"; Cat="Hô hấp"; Qty=2180; Min=20; Man="Dược Hậu Giang"; Exp="'2026-10-10'"; Act=1 },
    @{ Name="Metoprolol 50mg"; Price=115000; Unit="Hộp"; Cat="Tim mạch"; Qty=3100; Min=30; Man="Boston"; Exp="'2027-02-28'"; Act=1 },
    @{ Name="Esomeprazole 40mg"; Price=220000; Unit="Hộp"; Cat="Tiêu hóa"; Qty=2130; Min=25; Man="Stella"; Exp="'2026-09-05'"; Act=1 },
    @{ Name="Gliclazide 60mg"; Price=140000; Unit="Hộp"; Cat="Tiểu đường"; Qty=2950; Min=30; Man="Hasan"; Exp="'2027-08-25'"; Act=1 },
    @{ Name="Losartan 50mg"; Price=125000; Unit="Hộp"; Cat="Tim mạch"; Qty=2720; Min=25; Man="Dược Hậu Giang"; Exp="'2026-08-22'"; Act=1 },
    @{ Name="Prednisolone 5mg"; Price=50000; Unit="Hộp"; Cat="Kháng viêm"; Qty=2040; Min=10; Man="Stella"; Exp="'2026-04-20'"; Act=0 }, # Expired, Inactive
    @{ Name="Methylprednisolone 16mg"; Price=160000; Unit="Hộp"; Cat="Kháng viêm"; Qty=3050; Min=40; Man="Boston"; Exp="'2027-03-15'"; Act=1 },
    @{ Name="Clopidogrel 75mg"; Price=210000; Unit="Hộp"; Cat="Tim mạch"; Qty=2140; Min=20; Man="Hasan"; Exp="'2026-09-12'"; Act=1 },
    @{ Name="Pantoprazole 40mg"; Price=180000; Unit="Hộp"; Cat="Tiêu hóa"; Qty=2250; Min=30; Man="Dược Hậu Giang"; Exp="'2027-04-30'"; Act=1 },
    @{ Name="Singulair 10mg"; Price=350000; Unit="Hộp"; Cat="Hô hấp"; Qty=2080; Min=15; Man="MSD"; Exp="'2026-07-28'"; Act=1 },
    @{ Name="Telfast 180mg"; Price=190000; Unit="Hộp"; Cat="Dị ứng"; Qty=2120; Min=20; Man="Sanofi"; Exp="'2027-06-15'"; Act=1 },
    @{ Name="Panadol Extra"; Price=85000; Unit="Hộp"; Cat="Giảm đau, hạ sốt"; Qty=4500; Min=100; Man="GSK"; Exp="'2028-02-28'"; Act=1 },
    @{ Name="Decolgen Forte"; Price=42000; Unit="Hộp"; Cat="Trị cảm cúm"; Qty=3800; Min=50; Man="United Pharma"; Exp="'2026-09-08'"; Act=1 },
    @{ Name="Strepsils Orange"; Price=35000; Unit="Hộp"; Cat="Trị đau họng"; Qty=2350; Min=30; Man="Reckitt"; Exp="'2027-09-15'"; Act=1 },
    @{ Name="Maalox"; Price=65000; Unit="Hộp"; Cat="Tiêu hóa"; Qty=2400; Min=25; Man="Sanofi"; Exp="'2026-08-01'"; Act=1 },
    @{ Name="Smecta"; Price=110000; Unit="Hộp"; Cat="Tiêu hóa"; Qty=2500; Min=30; Man="Ipsen"; Exp="'2027-12-10'"; Act=1 },
    @{ Name="Enterogermina"; Price=280000; Unit="Hộp"; Cat="Men tiêu hóa"; Qty=2300; Min=20; Man="Sanofi"; Exp="'2026-09-30'"; Act=1 },
    @{ Name="Eugica Green"; Price=55000; Unit="Hộp"; Cat="Hô hấp"; Qty=2600; Min=40; Man="Mega We Care"; Exp="'2026-10-05'"; Act=1 },
    @{ Name="Ameflu"; Price=45000; Unit="Hộp"; Cat="Trị cảm cúm"; Qty=3200; Min=50; Man="OPV"; Exp="'2027-10-20'"; Act=1 },
    @{ Name="Hapacol 250"; Price=38000; Unit="Hộp"; Cat="Hạ sốt trẻ em"; Qty=2085; Min=20; Man="Dược Hậu Giang"; Exp="'2026-05-15'"; Act=0 }, # Expired, Inactive
    @{ Name="Efferalgan 500mg"; Price=70000; Unit="Hộp"; Cat="Giảm đau, hạ sốt"; Qty=4200; Min=100; Man="Bristol-Myers Squibb"; Exp="'2028-05-12'"; Act=1 },
    @{ Name="Gaviscon Dual Action"; Price=160000; Unit="Hộp"; Cat="Tiêu hóa"; Qty=2450; Min=30; Man="Reckitt"; Exp="'2026-08-25'"; Act=1 },
    @{ Name="Smectago"; Price=135000; Unit="Hộp"; Cat="Tiêu hóa"; Qty=2210; Min=20; Man="Ipsen"; Exp="'2026-10-15'"; Act=1 },
    @{ Name="Berberin 50mg"; Price=15000; Unit="Lọ"; Cat="Tiêu hóa"; Qty=5000; Min=100; Man="OPC"; Exp="'2027-12-30'"; Act=1 },
    @{ Name="Duo-Plavin 75mg/100mg"; Price=650000; Unit="Hộp"; Cat="Tim mạch"; Qty=2050; Min=10; Man="Sanofi"; Exp="'2026-09-02'"; Act=1 },
    @{ Name="Co-Diovan 80/12.5mg"; Price=380000; Unit="Hộp"; Cat="Tim mạch"; Qty=2110; Min=15; Man="Novartis"; Exp="'2026-08-08'"; Act=1 },
    @{ Name="Concor 5mg"; Price=145000; Unit="Hộp"; Cat="Tim mạch"; Qty=2880; Min=30; Man="Merck"; Exp="'2027-04-15'"; Act=1 },
    @{ Name="Vastarel MR 35mg"; Price=185000; Unit="Hộp"; Cat="Tim mạch"; Qty=3020; Min=40; Man="Servier"; Exp="'2027-08-20'"; Act=1 },
    @{ Name="Diamicron MR 60mg"; Price=250000; Unit="Hộp"; Cat="Tiểu đường"; Qty=2940; Min=30; Man="Servier"; Exp="'2026-10-25'"; Act=1 },
    @{ Name="Glucophage 500mg"; Price=80000; Unit="Hộp"; Cat="Tiểu đường"; Qty=3300; Min=50; Man="Merck"; Exp="'2027-02-10'"; Act=1 },
    @{ Name="Jardiance 10mg"; Price=850000; Unit="Hộp"; Cat="Tiểu đường"; Qty=2040; Min=10; Man="Boehringer Ingelheim"; Exp="'2026-08-18'"; Act=1 },
    @{ Name="Lipitor 20mg"; Price=490000; Unit="Hộp"; Cat="Mỡ máu"; Qty=2120; Min=15; Man="Pfizer"; Exp="'2026-09-15'"; Act=1 },
    @{ Name="Crestor 10mg"; Price=420000; Unit="Hộp"; Cat="Mỡ máu"; Qty=2180; Min=20; Man="AstraZeneca"; Exp="'2026-08-30'"; Act=1 },
    @{ Name="Fenofibrate 160mg"; Price=110000; Unit="Hộp"; Cat="Mỡ máu"; Qty=2700; Min=30; Man="Hasan"; Exp="'2027-01-20'"; Act=1 },
    @{ Name="Colchicine 1mg"; Price=120000; Unit="Hộp"; Cat="Trị Gút"; Qty=2150; Min=15; Man="Stella"; Exp="'2026-10-12'"; Act=1 },
    @{ Name="Voltaren 75mg"; Price=35000; Unit="Ống"; Cat="Giảm đau, kháng viêm"; Qty=2500; Min=50; Man="Novartis"; Exp="'2026-08-04'"; Act=1 },
    @{ Name="Celebrex 200mg"; Price=390000; Unit="Hộp"; Cat="Giảm đau, kháng viêm"; Qty=2350; Min=20; Man="Pfizer"; Exp="'2027-05-15'"; Act=1 },
    @{ Name="Arcoxia 90mg"; Price=290000; Unit="Hộp"; Cat="Giảm đau, kháng viêm"; Qty=2220; Min=20; Man="MSD"; Exp="'2026-09-18'"; Act=1 },
    @{ Name="Nurofen Kids"; Price=125000; Unit="Chai"; Cat="Giảm đau hạ sốt"; Qty=2110; Min=15; Man="Reckitt"; Exp="'2026-08-12'"; Act=1 },
    @{ Name="Hapacol 650"; Price=65000; Unit="Hộp"; Cat="Giảm đau, hạ sốt"; Qty=3600; Min=50; Man="DHG"; Exp="'2027-11-30'"; Act=1 },
    @{ Name="Klamentin 875/125"; Price=190000; Unit="Hộp"; Cat="Kháng sinh"; Qty=2190; Min=20; Man="DHG"; Exp="'2026-10-20'"; Act=1 },
    @{ Name="Clarithromycin 500mg"; Price=175000; Unit="Hộp"; Cat="Kháng sinh"; Qty=2140; Min=15; Man="Stella"; Exp="'2027-03-20'"; Act=1 },
    @{ Name="Augmentin 1g"; Price=350000; Unit="Hộp"; Cat="Kháng sinh"; Qty=2320; Min=25; Man="GSK"; Exp="'2027-09-08'"; Act=1 },
    @{ Name="Bactrim"; Price=80000; Unit="Hộp"; Cat="Kháng sinh"; Qty=2130; Min=15; Man="Roche"; Exp="'2026-09-09'"; Act=1 },
    @{ Name="Symbicort Turbuhaler"; Price=620000; Unit="Lọ"; Cat="Hô hấp"; Qty=2060; Min=10; Man="AstraZeneca"; Exp="'2026-08-14'"; Act=1 },
    @{ Name="Seretide Evohaler"; Price=420000; Unit="Lọ"; Cat="Hô hấp"; Qty=2075; Min=10; Man="GSK"; Exp="'2026-10-02'"; Act=1 },
    @{ Name="Aerius 5mg"; Price=125000; Unit="Hộp"; Cat="Dị ứng"; Qty=2420; Min=30; Man="MSD"; Exp="'2027-04-18'"; Act=1 },
    @{ Name="Claritin 10mg"; Price=115000; Unit="Hộp"; Cat="Dị ứng"; Qty=2310; Min=25; Man="Bayer"; Exp="'2026-09-22'"; Act=1 },
    @{ Name="Otrivin 0.1%"; Price=55000; Unit="Lọ"; Cat="Nhỏ mũi"; Qty=2800; Min=30; Man="Novartis"; Exp="'2026-08-09'"; Act=1 },
    @{ Name="Naphazolin 0.05%"; Price=8000; Unit="Lọ"; Cat="Nhỏ mũi"; Qty=3500; Min=50; Man="DHG"; Exp="'2027-08-10'"; Act=1 },
    @{ Name="Systane Ultra"; Price=95000; Unit="Lọ"; Cat="Nhỏ mắt"; Qty=3100; Min=40; Man="Alcon"; Exp="'2027-02-15'"; Act=1 },
    @{ Name="Viroto"; Price=45000; Unit="Lọ"; Cat="Nhỏ mắt"; Qty=2900; Min=30; Man="Rohto"; Exp="'2026-09-10'"; Act=1 },
    @{ Name="Tobrex 0.3%"; Price=68000; Unit="Lọ"; Cat="Nhỏ mắt"; Qty=2400; Min=20; Man="Alcon"; Exp="'2026-08-20'"; Act=1 },
    @{ Name="Bepanthen"; Price=85000; Unit="Tuýp"; Cat="Da liễu"; Qty=2650; Min=30; Man="Bayer"; Exp="'2027-05-20'"; Act=1 },
    @{ Name="Skinoren Cream"; Price=320000; Unit="Tuýp"; Cat="Da liễu"; Qty=2120; Min=15; Man="Leo Pharma"; Exp="'2026-09-25'"; Act=1 },
    @{ Name="Dermovate"; Price=95000; Unit="Tuýp"; Cat="Da liễu"; Qty=2350; Min=20; Man="GSK"; Exp="'2026-08-03'"; Act=1 },
    @{ Name="Calamine"; Price=65000; Unit="Chai"; Cat="Da liễu"; Qty=2240; Min=20; Man="Stella"; Exp="'2027-03-12'"; Act=1 },
    @{ Name="Eumovate"; Price=75000; Unit="Tuýp"; Cat="Da liễu"; Qty=2410; Min=25; Man="GSK"; Exp="'2027-08-15'"; Act=1 },
    @{ Name="Neurobion"; Price=110000; Unit="Hộp"; Cat="Thần kinh"; Qty=3050; Min=40; Man="Merck"; Exp="'2027-06-30'"; Act=1 },
    @{ Name="Ginkgo Biloba"; Price=150000; Unit="Hộp"; Cat="Tuần hoàn não"; Qty=3800; Min=50; Man="Mason"; Exp="'2027-12-18'"; Act=1 },
    @{ Name="Nootropil 800mg"; Price=210000; Unit="Hộp"; Cat="Thần kinh"; Qty=2140; Min=15; Man="UCB"; Exp="'2026-09-14'"; Act=1 },
    @{ Name="Magne B6"; Price=95000; Unit="Hộp"; Cat="Vitamin & Khoáng chất"; Qty=4200; Min=100; Man="Sanofi"; Exp="'2027-10-15'"; Act=1 },
    @{ Name="Calcium Carbonate"; Price=160000; Unit="Hộp"; Cat="Vitamin & Khoáng chất"; Qty=3400; Min=50; Man="Sanofi"; Exp="'2027-04-20'"; Act=1 },
    @{ Name="Enervon"; Price=72000; Unit="Hộp"; Cat="Vitamin & Khoáng chất"; Qty=2900; Min=45; Man="United Pharma"; Exp="'2026-08-07'"; Act=1 },
    @{ Name="Obimin"; Price=195000; Unit="Hộp"; Cat="Vitamin & Khoáng chất"; Qty=2500; Min=25; Man="United Pharma"; Exp="'2026-10-22'"; Act=1 },
    @{ Name="Decumar"; Price=75000; Unit="Tuýp"; Cat="Da liễu"; Qty=2380; Min=20; Man="CVI"; Exp="'2027-09-30'"; Act=1 },
    @{ Name="Gliclada 30mg"; Price=115000; Unit="Hộp"; Cat="Tiểu đường"; Qty=2720; Min=25; Man="Krka"; Exp="'2026-09-11'"; Act=1 },
    @{ Name="Galvus Met"; Price=450000; Unit="Hộp"; Cat="Tiểu đường"; Qty=2110; Min=15; Man="Novartis"; Exp="'2026-08-21'"; Act=1 },
    @{ Name="Glucovance"; Price=210000; Unit="Hộp"; Cat="Tiểu đường"; Qty=2350; Min=20; Man="Merck"; Exp="'2027-03-25'"; Act=1 },
    @{ Name="Lantus Solostar"; Price=420000; Unit="Lọ"; Cat="Tiểu đường"; Qty=2120; Min=15; Man="Sanofi"; Exp="'2026-08-30'"; Act=1 },
    @{ Name="Plavix 75mg"; Price=550000; Unit="Hộp"; Cat="Tim mạch"; Qty=2150; Min=15; Man="Sanofi"; Exp="'2027-01-30'"; Act=1 },
    @{ Name="Micardis 40mg"; Price=310000; Unit="Hộp"; Cat="Tim mạch"; Qty=2240; Min=15; Man="Boehringer Ingelheim"; Exp="'2026-09-08'"; Act=1 },
    @{ Name="Coveram 5mg/5mg"; Price=360000; Unit="Hộp"; Cat="Tim mạch"; Qty=2310; Min=20; Man="Servier"; Exp="'2027-05-15'"; Act=1 },
    @{ Name="Imdur 60mg"; Price=260000; Unit="Hộp"; Cat="Tim mạch"; Qty=2150; Min=15; Man="AstraZeneca"; Exp="'2026-08-11'"; Act=1 }
)

# Convert original 100 meds array into HashSet of names
$usedNames = New-Object System.Collections.Generic.HashSet[string]
foreach ($m in $originalMeds) {
    [void]$usedNames.Add($m.Name)
}

# Generate 900 new unique medicines
$newMedicinesList = New-Object System.Collections.Generic.List[object]
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
        
        # Every generated medicine has at least 2000 quantity
        $stock = $rand.Next(200, 450) * 10 
        $minStock = $rand.Next(10, 100)
        $manufacturer = $manufacturers[$rand.Next($manufacturers.Length)]
        
        $dateType = $rand.Next(1, 11)
        $expiryDateStr = "NULL"
        $isActive = 1
        
        if ($dateType -eq 1) {
            # Expired: set ExpiryDate to past, but make it inactive IsActive = 0
            $expiryDateStr = "'" + (Get-Date).AddDays(-$rand.Next(10, 300)).ToString("yyyy-MM-dd") + "'"
            $isActive = 0
        } elseif ($dateType -eq 2) {
            # Expiring in 1 month: active IsActive = 1
            $expiryDateStr = "'" + (Get-Date).AddDays($rand.Next(1, 29)).ToString("yyyy-MM-dd") + "'"
            $isActive = 1
        } elseif ($dateType -eq 3) {
            # Expiring in 3 months: active IsActive = 1
            $expiryDateStr = "'" + (Get-Date).AddDays($rand.Next(31, 89)).ToString("yyyy-MM-dd") + "'"
            $isActive = 1
        } else {
            # Far future: active IsActive = 1
            $expiryDateStr = "'" + (Get-Date).AddDays($rand.Next(365, 1000)).ToString("yyyy-MM-dd") + "'"
            $isActive = 1
        }
        
        $medObj = @{ Name=$name; Price=$price; Unit=$unit; Cat=$category; Qty=$stock; Min=$minStock; Man=$manufacturer; Exp=$expiryDateStr; Act=$isActive }
        $newMedicinesList.Add($medObj)
        $count++
    }
}

# Combine all 1000 medicines into a single array
$all1000Meds = New-Object System.Collections.Generic.List[object]
foreach ($m in $originalMeds) {
    $all1000Meds.Add($m)
}
foreach ($m in $newMedicinesList) {
    $all1000Meds.Add($m)
}

# Now, we select EXACTLY 7 medicines and set their StockQuantity to a random number between 400 and 600!
# We will pick 7 active non-expired medicines from the list.
$pickedCount = 0
$i = 0
while ($pickedCount -lt 7 -and $i -lt $all1000Meds.Count) {
    $med = $all1000Meds[$i]
    # Check if medicine is active and not expired
    $isExpired = $false
    if ($med.Exp -ne "NULL") {
        $expDateStr = $med.Exp.Replace("'", "")
        $expDate = [DateTime]::ParseExact($expDateStr, "yyyy-MM-dd", $null)
        if ($expDate -lt (Get-Date)) {
            $isExpired = $true
        }
    }
    if ($med.Act -eq 1 -and -not $isExpired) {
        $med.Qty = $rand.Next(400, 601) # Set random between 400 and 600
        $pickedCount++
        Write-Host "Set nearly-out-of-stock for: $($med.Name) to $($med.Qty)"
    }
    $i++
}

# Format the full SQL INSERT statement
$sqlInsertValues = New-Object System.Collections.Generic.List[string]
foreach ($med in $all1000Meds) {
    $sqlLine = "(N'$($med.Name)', $($med.Price), N'$($med.Unit)', N'$($med.Cat)', $($med.Qty), $($med.Min), N'$($med.Man)', $($med.Exp), $($med.Act))"
    $sqlInsertValues.Add($sqlLine)
}

$sqlInsertStatement = "INSERT INTO Medicines (Name, Price, Unit, Category, StockQuantity, MinStock, Manufacturer, ExpiryDate, IsActive) VALUES `r`n" + ($sqlInsertValues -join ",`r`n") + ";"

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
