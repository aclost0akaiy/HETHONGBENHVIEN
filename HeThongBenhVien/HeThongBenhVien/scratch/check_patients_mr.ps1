# check_patients_mr.ps1
$connStr = "Data Source=PC-202212270109\SQLEXPRESS;Initial Catalog=QuanLyBenhVienDb;Integrated Security=True;TrustServerCertificate=True"
$c = New-Object System.Data.SqlClient.SqlConnection($connStr)
$c.Open()
$cmd = $c.CreateCommand()

Write-Host "--- Total counts ---"
$cmd.CommandText = "SELECT COUNT(*) FROM Patients"
$ptCount = $cmd.ExecuteScalar()
Write-Host "Total Patients: $ptCount"

$cmd.CommandText = "SELECT COUNT(*) FROM Appointments"
$apCount = $cmd.ExecuteScalar()
Write-Host "Total Appointments: $apCount"

$cmd.CommandText = "SELECT COUNT(*) FROM MedicalRecords"
$mrCount = $cmd.ExecuteScalar()
Write-Host "Total MedicalRecords: $mrCount"

Write-Host "`n--- Patient BN_SEED_001 info ---"
$cmd.CommandText = "SELECT Id, FullName, PatientCode FROM Patients WHERE PatientCode = 'BN_SEED_001'"
$r = $cmd.ExecuteReader()
if ($r.Read()) {
    $patientId = $r["Id"]
    Write-Host "Patient ID: $patientId, Name: $($r["FullName"]), Code: $($r["PatientCode"])"
    $r.Close()

    $cmd.CommandText = "SELECT Id, Status, DoctorId FROM Appointments WHERE PatientId = $patientId"
    $r2 = $cmd.ExecuteReader()
    while ($r2.Read()) {
        Write-Host "Appointment ID: $($r2["Id"]), Status: $($r2["Status"]), DoctorId: $($r2["DoctorId"])"
    }
    $r2.Close()

    $cmd.CommandText = "SELECT MR.Id, MR.AppointmentId, MR.Diagnosis FROM MedicalRecords MR JOIN Appointments A ON MR.AppointmentId = A.Id WHERE A.PatientId = $patientId"
    $r3 = $cmd.ExecuteReader()
    while ($r3.Read()) {
        Write-Host "MedicalRecord ID: $($r3["Id"]), AppointmentId: $($r3["AppointmentId"]), Diagnosis: $($r3["Diagnosis"])"
    }
    $r3.Close()
} else {
    Write-Host "Patient BN_SEED_001 not found!"
    $r.Close()
}

$c.Close()
