$connString = "Data Source=PC-202212270109\SQLEXPRESS;Initial Catalog=QuanLyBenhVienDb;Integrated Security=True;TrustServerCertificate=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connString)
$conn.Open()
$cmd = $conn.CreateCommand()
$cmd.CommandText = "SELECT TOP 5 A.Id, P.FullName, A.AppointmentTime, A.Status FROM Appointments A JOIN Patients P ON A.PatientId = P.Id WHERE A.DoctorId = 6 ORDER BY A.AppointmentTime ASC"
$reader = $cmd.ExecuteReader()
while ($reader.Read()) {
    Write-Output "Id: $($reader.GetValue(0)) - Name: $($reader.GetValue(1)) - Time: $($reader.GetValue(2)) - Status: $($reader.GetValue(3))"
}
$conn.Close()
