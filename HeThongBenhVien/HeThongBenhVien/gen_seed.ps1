$sql = ""

$sql += "USE QuanLyBenhVienDb;`r`nGO`r`n`r`n"
# 7 in Danh Sach (Appointments with Status = 1 - Đang chờ)
for ($i = 1; $i -le 7; $i++) {
    $sql += "INSERT INTO Patients (FullName, Gender, Age, PatientCode, Allergies) VALUES (N'Bệnh nhân chờ khám $i', N'Nam', 30, 'DS$i', '');`r`n"
    $sql += "DECLARE @Pid$i INT = SCOPE_IDENTITY();`r`n"
    $sql += "INSERT INTO Appointments (PatientId, Reason, AppointmentTime, Status) VALUES (@Pid$i, N'Đau đầu', GETDATE(), 1);`r`n"
}

# 20 in HoSoBenhAn (MedicalRecords created, Appointment Status = 4 - Đã khám xong)
for ($i = 1; $i -le 20; $i++) {
    $sql += "INSERT INTO Patients (FullName, Gender, Age, PatientCode, Allergies) VALUES (N'Bệnh nhân khám xong $i', N'Nữ', 40, 'HS$i', '');`r`n"
    $sql += "DECLARE @Pid_HS$i INT = SCOPE_IDENTITY();`r`n"
    $sql += "INSERT INTO Appointments (PatientId, Reason, AppointmentTime, Status) VALUES (@Pid_HS$i, N'Khám định kỳ', GETDATE(), 4);`r`n"
    $sql += "DECLARE @Aid_HS$i INT = SCOPE_IDENTITY();`r`n"
    $sql += "INSERT INTO MedicalRecords (AppointmentId, Symptoms, Diagnosis, TreatmentPlan, CreatedAt, IsLocked) VALUES (@Aid_HS$i, N'Triệu chứng $i', N'Chẩn đoán $i', N'Điều trị $i', GETDATE(), 0);`r`n"
}

# 50 in GiuongBenh (MedicalRecords with AdmissionDate, DepartmentId)
# assuming DepartmentId 1, 2, 3 exist.
for ($i = 1; $i -le 50; $i++) {
    $dept = ($i % 3) + 1
    $bed = $i
    $sql += "INSERT INTO Patients (FullName, Gender, Age, PatientCode, Allergies) VALUES (N'Bệnh nhân nội trú $i', N'Nam', 50, 'NT$i', '');`r`n"
    $sql += "DECLARE @Pid_NT$i INT = SCOPE_IDENTITY();`r`n"
    $sql += "INSERT INTO Appointments (PatientId, Reason, AppointmentTime, Status) VALUES (@Pid_NT$i, N'Cấp cứu', GETDATE(), 4);`r`n"
    $sql += "DECLARE @Aid_NT$i INT = SCOPE_IDENTITY();`r`n"
    $sql += "INSERT INTO MedicalRecords (AppointmentId, Symptoms, Diagnosis, TreatmentPlan, AdmissionDate, DepartmentId, BedNumber, CreatedAt, IsLocked) VALUES (@Aid_NT$i, N'Triệu chứng nặng', N'Nhập viện', N'Theo dõi', GETDATE(), $dept, $bed, GETDATE(), 0);`r`n"
}
$sql += "GO`r`n"

$sql | Out-File -FilePath "insert_demo_data.sql" -Encoding UTF8
