USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'QuanLyBenhVienDb')
BEGIN
    CREATE DATABASE QuanLyBenhVienDb;
END
GO

USE QuanLyBenhVienDb;
GO

-- Xóa bảng nếu đã tồn tại để tránh lỗi khi chạy lại script
IF OBJECT_ID('VitalSigns', 'U') IS NOT NULL DROP TABLE VitalSigns;
IF OBJECT_ID('LabTests', 'U') IS NOT NULL DROP TABLE LabTests;
IF OBJECT_ID('PrescriptionDetails', 'U') IS NOT NULL DROP TABLE PrescriptionDetails;
IF OBJECT_ID('Prescriptions', 'U') IS NOT NULL DROP TABLE Prescriptions;
IF OBJECT_ID('MedicalRecords', 'U') IS NOT NULL DROP TABLE MedicalRecords;
IF OBJECT_ID('Appointments', 'U') IS NOT NULL DROP TABLE Appointments;
IF OBJECT_ID('Patients', 'U') IS NOT NULL DROP TABLE Patients;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
GO

-- 1. Tạo bảng Bệnh Nhân
CREATE TABLE Patients (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Gender NVARCHAR(50) NOT NULL DEFAULT N'Nam',
    Age INT NOT NULL,
    PatientCode NVARCHAR(20) NOT NULL
);
GO

-- 1.5 Tạo bảng Người dùng (Tài khoản)
CREATE TABLE Users (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    FullName NVARCHAR(100) NULL
);
GO

-- 2. Tạo bảng Lịch Khám (Appointments)
CREATE TABLE Appointments (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    Reason NVARCHAR(200) NOT NULL,
    AppointmentTime DATETIME2 NOT NULL,
    Status INT NOT NULL, -- 0 - Chưa đến, 1 - Đang chờ, 2 - Đang khám, 3 - Có KQ XN, 4 - Đã khám xong
    CONSTRAINT FK_Appointments_Patients FOREIGN KEY (PatientId) REFERENCES Patients(Id) ON DELETE CASCADE
);
GO

-- 3. Tạo bảng Hồ sơ bệnh án (MedicalRecords)
CREATE TABLE MedicalRecords (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    Symptoms NVARCHAR(MAX) NOT NULL,
    Vitals NVARCHAR(MAX) NOT NULL,
    Diagnosis NVARCHAR(MAX) NOT NULL,
    TreatmentPlan NVARCHAR(MAX) NOT NULL,
    Notes NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_MedicalRecords_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE
);
GO

-- 3.1. Tạo bảng Đơn thuốc (Prescriptions)
CREATE TABLE Prescriptions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MedicalRecordId INT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(50) NOT NULL DEFAULT N'Đã kê đơn',
    CONSTRAINT FK_Prescriptions_MedicalRecords FOREIGN KEY (MedicalRecordId) REFERENCES MedicalRecords(Id) ON DELETE CASCADE
);
GO

CREATE TABLE PrescriptionDetails (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PrescriptionId INT NOT NULL,
    MedicineName NVARCHAR(200) NOT NULL,
    Quantity INT NOT NULL,
    Unit NVARCHAR(50) NOT NULL,
    DosageInstruction NVARCHAR(255) NOT NULL,
    Price DECIMAL(18,2) NOT NULL DEFAULT 0,
    CONSTRAINT FK_PrescriptionDetails_Prescriptions FOREIGN KEY (PrescriptionId) REFERENCES Prescriptions(Id) ON DELETE CASCADE
);
GO

-- 3.1.5 Tạo bảng Đơn vị thuốc (MedicineUnits)
CREATE TABLE MedicineUnits (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UnitName NVARCHAR(50) NOT NULL,
    DefaultPrice DECIMAL(18,2) NOT NULL DEFAULT 0
);
GO

INSERT INTO MedicineUnits (UnitName, DefaultPrice) VALUES 
(N'Viên', 5000), (N'Vỉ', 8000), (N'Lọ', 20000), 
(N'Hộp', 60000), (N'Ống', 10000), (N'Cái', 20000), 
(N'Tuýp', 15000), (N'Gói', 25000), (N'Chai', 15000);
GO

-- 3.2. Tạo bảng Xét nghiệm (LabTests)
CREATE TABLE LabTests (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MedicalRecordId INT NOT NULL,
    TestName NVARCHAR(200) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT N'Chờ xét nghiệm',
    Result NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CompletedAt DATETIME2 NULL,
    CONSTRAINT FK_LabTests_MedicalRecords FOREIGN KEY (MedicalRecordId) REFERENCES MedicalRecords(Id) ON DELETE CASCADE
);
GO

-- 3.3. Tạo bảng Sinh hiệu (VitalSigns)
CREATE TABLE VitalSigns (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    AppointmentId INT NOT NULL,
    Pulse NVARCHAR(50) NULL,
    Temperature NVARCHAR(50) NULL,
    BloodPressure NVARCHAR(50) NULL,
    SpO2 NVARCHAR(50) NULL,
    NurseName NVARCHAR(100) NULL,
    RecordedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_VitalSigns_Appointments FOREIGN KEY (AppointmentId) REFERENCES Appointments(Id) ON DELETE CASCADE
);
GO

-- 3. Thêm Dữ Liệu Mẫu (Seed Data) cho Patients
SET IDENTITY_INSERT Patients ON;
INSERT INTO Patients (Id, FullName, Gender, Age, PatientCode)
VALUES 
(1, N'Trần Văn Lâm', N'Nam', 45, N'BN1293'),
(2, N'Hoàng Thị Mai', N'Nữ', 32, N'BN1294'),
(3, N'Phạm Quốc Quân', N'Nam', 58, N'BN1295');
SET IDENTITY_INSERT Patients OFF;
GO

-- 4. Thêm Dữ Liệu Mẫu (Seed Data) cho Appointments
SET IDENTITY_INSERT Appointments ON;

DECLARE @Today DATE = GETDATE();

INSERT INTO Appointments (Id, PatientId, Reason, AppointmentTime, Status)
VALUES 
(1, 1, N'Đau thắt ngực, khó thở', DATEADD(MINUTE, 30, DATEADD(HOUR, 8, CAST(@Today AS DATETIME2))), 1),
(2, 2, N'Tái khám huyết áp', DATEADD(HOUR, 9, CAST(@Today AS DATETIME2)), 3),
(3, 3, N'Đánh trống ngực liên tục', DATEADD(MINUTE, 30, DATEADD(HOUR, 9, CAST(@Today AS DATETIME2))), 0);

SET IDENTITY_INSERT Appointments OFF;
GO

-- 5. Thêm Dữ Liệu Mẫu (Seed Data) cho Users
SET IDENTITY_INSERT Users ON;
INSERT INTO Users (Id, Username, Password, Role, FullName)
VALUES 
(1, N'admin', N'123', N'Admin', N'Quản trị viên hệ thống'),
(2, N'doctor', N'123', N'Doctor', N'BS. Nguyễn Văn A');
SET IDENTITY_INSERT Users OFF;
GO

ALTER TABLE [QuanLyBenhVienDb].[dbo].[Users]
ADD 
    [Email] NVARCHAR(100),
    [SDT] VARCHAR(20);
-- Cập nhật thông tin cho Quản trị viên (Id = 1)
UPDATE [QuanLyBenhVienDb].[dbo].[Users]
SET 
    [Email] = 'admin@benhvien.com',
    [SDT] = '0901111222'
WHERE [Id] = 1;

-- Cập nhật thông tin cho Bác sĩ Nguyễn Văn A (Id = 2)
UPDATE [QuanLyBenhVienDb].[dbo].[Users]
SET 
    [Email] = 'nguyenvana.bs@gmail.com',
    [SDT] = '0987654321'
WHERE [Id] = 2;

IF OBJECT_ID('WorkSchedules', 'U') IS NOT NULL DROP TABLE WorkSchedules;
-- =============================================
-- Tạo bảng Lịch Làm Việc (WorkSchedules)
-- =============================================


-- 1. Thêm 5 nhân sự mẫu vào bảng Users
INSERT INTO Users (Username, Password, FullName, Role, Email, SDT)
VALUES 
('bs_a', '123456', N'BS. Nguyễn Văn A', 'Doctor', 'bs.a@benhvien.com', '0901234567'),
('bs_b', '123456', N'BS. Trần Thị B', 'Doctor', 'bs.b@benhvien.com', '0902345678'),
('bs_c', '123456', N'BS. Lê Văn C', 'Doctor', 'bs.c@benhvien.com', '0903456789'),
('bs_d', '123456', N'BS. Phạm Văn D', 'Doctor', 'bs.d@benhvien.com', '0904567890'),
('admin_e', '123456', N'Quản trị viên E', 'Admin', 'admin.e@benhvien.com', '0905678901');
GO

-- Sau khi chạy xong đoạn này, hãy bôi đen chạy lại đoạn INSERT bảng WorkSchedules của tin nhắn trước!

SELECT Id, FullName FROM Users;

USE QuanLyBenhVienDb;
GO

-- Lệnh này sẽ xóa sạch dữ liệu và reset lại ID về 1
TRUNCATE TABLE WorkSchedules;
GO

CREATE TABLE WorkSchedules (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,                -- Thay cho "Tên": Liên kết với ID của nhân sự trong bảng Users
    WorkTime NVARCHAR(50) NOT NULL,     -- Thời gian (VD: '07:00 - 15:00')
    ShiftName NVARCHAR(50) NOT NULL,    -- Ca (VD: N'Ca Sáng', N'Ca Chiều')
    WorkDate DATE NOT NULL,             -- Ngày (VD: '2026-05-12')
    WeekNumber INT NOT NULL,            -- Tuần (VD: 20)
    MonthNumber INT NOT NULL,           -- Tháng (VD: 5)
    YearNumber INT NOT NULL,            -- Năm (VD: 2026)

    -- Khai báo khóa ngoại liên kết với bảng Users
    -- Khi xóa một User, bạn có thể thiết lập báo lỗi hoặc tự động xóa lịch (ở đây mình để mặc định)
    CONSTRAINT FK_WorkSchedules_Users FOREIGN KEY (UserId) REFERENCES Users(Id)
);
GO

USE QuanLyBenhVienDb;
GO

-- Xóa dữ liệu cũ nếu có để tránh trùng lặp khi chạy lại (tuỳ chọn)
-- TRUNCATE TABLE WorkSchedules;

USE QuanLyBenhVienDb;
GO

-- 1. Xóa sạch dữ liệu cũ không khớp form
TRUNCATE TABLE WorkSchedules;
GO

-- 2. Thêm dữ liệu mới với ShiftName chuẩn "Ca 1", "Ca 2"... để khớp với code lưới
INSERT INTO WorkSchedules (UserId, WorkTime, ShiftName, WorkDate, WeekNumber, MonthNumber, YearNumber)
VALUES
-- Ngày 12/05/2026 (Thứ 3)
(1, '06:00 - 09:00', N'Ca 1 (06:00 - 09:00)', '2026-05-12', 20, 5, 2026),
(2, '09:00 - 12:00', N'Ca 2 (09:00 - 12:00)', '2026-05-12', 20, 5, 2026),
(3, '12:00 - 15:00', N'Ca 3 (12:00 - 15:00)', '2026-05-12', 20, 5, 2026),
(4, '15:00 - 18:00', N'Ca 4 (15:00 - 18:00)', '2026-05-12', 20, 5, 2026),

-- Ngày 13/05/2026 (Thứ 4)
(5, '06:00 - 09:00', N'Ca 1 (06:00 - 09:00)', '2026-05-13', 20, 5, 2026),
(1, '15:00 - 18:00', N'Ca 4 (15:00 - 18:00)', '2026-05-13', 20, 5, 2026),
(2, '18:00 - 21:00', N'Ca 5 (18:00 - 21:00)', '2026-05-13', 20, 5, 2026),

-- Ngày 14/05/2026 (Thứ 5)
(3, '09:00 - 12:00', N'Ca 2 (09:00 - 12:00)', '2026-05-14', 20, 5, 2026),
(4, '21:00 - 24:00', N'Ca 6 (21:00 - 24:00)', '2026-05-14', 20, 5, 2026),
(5, '00:00 - 06:00', N'Ca 7 (00:00 - 06:00)', '2026-05-14', 20, 5, 2026),

-- Ngày 15/05/2026 (Thứ 6)
(1, '12:00 - 15:00', N'Ca 3 (12:00 - 15:00)', '2026-05-15', 20, 5, 2026),
(2, '15:00 - 18:00', N'Ca 4 (15:00 - 18:00)', '2026-05-15', 20, 5, 2026),
(3, '18:00 - 21:00', N'Ca 5 (18:00 - 21:00)', '2026-05-15', 20, 5, 2026),

-- Ngày 16/05/2026 (Thứ 7)
(4, '06:00 - 09:00', N'Ca 1 (06:00 - 09:00)', '2026-05-16', 20, 5, 2026),
(5, '09:00 - 12:00', N'Ca 2 (09:00 - 12:00)', '2026-05-16', 20, 5, 2026),
(1, '00:00 - 06:00', N'Ca 7 (00:00 - 06:00)', '2026-05-16', 20, 5, 2026),

-- Ngày 17/05/2026 (Chủ Nhật)
(2, '12:00 - 15:00', N'Ca 3 (12:00 - 15:00)', '2026-05-17', 20, 5, 2026),
(3, '15:00 - 18:00', N'Ca 4 (15:00 - 18:00)', '2026-05-17', 20, 5, 2026),
(4, '21:00 - 24:00', N'Ca 6 (21:00 - 24:00)', '2026-05-17', 20, 5, 2026);
GO