$connStr = "Data Source=PC-202212270109\SQLEXPRESS;Initial Catalog=QuanLyBenhVienDb;Integrated Security=True;TrustServerCertificate=True"
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr)
$conn.Open()

function Run-Query($sql) {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
    $dt = New-Object System.Data.DataTable
    $adapter.Fill($dt) | Out-Null
    return $dt
}

Write-Output "=== Notifications (All Messages) ==="
Run-Query "SELECT Id, Message, Type, IsForPatient FROM Notifications" | Format-Table -Wrap | Out-String | Write-Output

$conn.Close()
