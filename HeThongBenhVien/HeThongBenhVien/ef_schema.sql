CREATE TABLE [BloodBanks] (
    [Id] int NOT NULL IDENTITY,
    [BagCode] nvarchar(20) NOT NULL,
    [BloodType] nvarchar(5) NOT NULL,
    [Component] nvarchar(20) NOT NULL,
    [VolumeMl] int NOT NULL,
    [DonorName] nvarchar(100) NOT NULL,
    [DonorPhone] nvarchar(20) NOT NULL,
    [CollectionDate] datetime2 NOT NULL,
    [ExpiryDate] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Notes] nvarchar(200) NOT NULL,
    CONSTRAINT [PK_BloodBanks] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Departments] (
    [Id] int NOT NULL IDENTITY,
    [DepartmentCode] nvarchar(50) NOT NULL,
    [DepartmentName] nvarchar(100) NOT NULL,
    [Description] nvarchar(200) NOT NULL,
    [HeadDoctor] nvarchar(100) NOT NULL,
    [TotalBeds] int NOT NULL,
    [OccupiedBeds] int NOT NULL,
    [Phone] nvarchar(10) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [HospitalFees] (
    [Id] int NOT NULL IDENTITY,
    [FeeCode] nvarchar(50) NOT NULL,
    [FeeName] nvarchar(200) NOT NULL,
    [Category] nvarchar(100) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [InsuranceCoverage] decimal(18,2) NOT NULL,
    [Description] nvarchar(500) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_HospitalFees] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [MedicalEquipments] (
    [Id] int NOT NULL IDENTITY,
    [EquipmentCode] nvarchar(max) NOT NULL,
    [EquipmentName] nvarchar(max) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [LastMaintenanceDate] datetime2 NOT NULL,
    CONSTRAINT [PK_MedicalEquipments] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [MedicalServices] (
    [Id] int NOT NULL IDENTITY,
    [ServiceCode] nvarchar(max) NOT NULL,
    [ServiceName] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_MedicalServices] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Medicines] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Unit] nvarchar(50) NOT NULL,
    [Category] nvarchar(100) NOT NULL,
    [StockQuantity] int NOT NULL,
    [MinStock] int NOT NULL,
    [Manufacturer] nvarchar(200) NOT NULL,
    [ExpiryDate] datetime2 NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Medicines] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [MedicineUnits] (
    [Id] int NOT NULL IDENTITY,
    [UnitName] nvarchar(50) NOT NULL,
    [DefaultPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_MedicineUnits] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Patients] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(100) NOT NULL,
    [Gender] nvarchar(max) NOT NULL,
    [Age] int NOT NULL,
    [PatientCode] nvarchar(20) NOT NULL,
    [Allergies] nvarchar(max) NULL,
    CONSTRAINT [PK_Patients] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [QualityReviews] (
    [Id] int NOT NULL IDENTITY,
    [Department] nvarchar(100) NOT NULL,
    [ReviewerName] nvarchar(100) NOT NULL,
    [ServiceScore] int NOT NULL,
    [CleanlinessScore] int NOT NULL,
    [StaffScore] int NOT NULL,
    [FacilityScore] int NOT NULL,
    [WaitTimeScore] int NOT NULL,
    [Comment] nvarchar(1000) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_QualityReviews] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Username] nvarchar(50) NOT NULL,
    [Password] nvarchar(255) NOT NULL,
    [Role] nvarchar(50) NOT NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Email] nvarchar(100) NOT NULL,
    [SDT] nvarchar(20) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Appointments] (
    [Id] int NOT NULL IDENTITY,
    [PatientId] int NOT NULL,
    [Reason] nvarchar(200) NOT NULL,
    [AppointmentTime] datetime2 NOT NULL,
    [Status] int NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Appointments_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [DiagnosticImages] (
    [Id] int NOT NULL IDENTITY,
    [RequestCode] nvarchar(20) NOT NULL,
    [PatientId] int NOT NULL,
    [ImageType] nvarchar(100) NOT NULL,
    [BodyPart] nvarchar(200) NOT NULL,
    [RequestedBy] nvarchar(100) NOT NULL,
    [Result] nvarchar(1000) NOT NULL,
    [Conclusion] nvarchar(500) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [RequestDate] datetime2 NOT NULL,
    [CompletedDate] datetime2 NULL,
    CONSTRAINT [PK_DiagnosticImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DiagnosticImages_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [InsuranceCards] (
    [Id] int NOT NULL IDENTITY,
    [CardNumber] nvarchar(20) NOT NULL,
    [PatientId] int NOT NULL,
    [RegisteredHospital] nvarchar(200) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [CoveragePercent] int NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_InsuranceCards] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_InsuranceCards_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Receptions] (
    [Id] int NOT NULL IDENTITY,
    [ReceptionCode] nvarchar(20) NOT NULL,
    [PatientId] int NOT NULL,
    [Department] nvarchar(100) NOT NULL,
    [Priority] nvarchar(50) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [QueueNumber] int NOT NULL,
    [Reason] nvarchar(500) NOT NULL,
    [CheckInTime] datetime2 NOT NULL,
    [CheckOutTime] datetime2 NULL,
    CONSTRAINT [PK_Receptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Receptions_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Surgeries] (
    [Id] int NOT NULL IDENTITY,
    [SurgeryCode] nvarchar(20) NOT NULL,
    [PatientId] int NOT NULL,
    [SurgeryName] nvarchar(200) NOT NULL,
    [SurgeryType] nvarchar(100) NOT NULL,
    [Surgeon] nvarchar(100) NOT NULL,
    [AssistantTeam] nvarchar(200) NOT NULL,
    [Anesthesia] nvarchar(100) NOT NULL,
    [OperatingRoom] nvarchar(50) NOT NULL,
    [ScheduledDate] datetime2 NOT NULL,
    [DurationMinutes] int NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Notes] nvarchar(1000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Surgeries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Surgeries_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [WorkSchedules] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [WorkTime] nvarchar(50) NOT NULL,
    [ShiftName] nvarchar(50) NOT NULL,
    [WorkDate] datetime2 NOT NULL,
    [WeekNumber] int NOT NULL,
    [MonthNumber] int NOT NULL,
    [YearNumber] int NOT NULL,
    CONSTRAINT [PK_WorkSchedules] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_WorkSchedules_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [MedicalRecords] (
    [Id] int NOT NULL IDENTITY,
    [AppointmentId] int NOT NULL,
    [Symptoms] nvarchar(max) NOT NULL,
    [Diagnosis] nvarchar(max) NOT NULL,
    [TreatmentPlan] nvarchar(max) NOT NULL,
    [Notes] nvarchar(max) NOT NULL,
    [DepartmentId] int NULL,
    [BedNumber] int NULL,
    [AdmissionDate] datetime2 NULL,
    [DischargeDate] datetime2 NULL,
    [RoomFee] decimal(18,2) NOT NULL,
    [SurgeonId] int NULL,
    [SurgeryFeeId] int NULL,
    [CreatedAt] datetime2 NOT NULL,
    [IsLocked] bit NOT NULL,
    [DigitalSignature] nvarchar(max) NULL,
    CONSTRAINT [PK_MedicalRecords] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_MedicalRecords_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_MedicalRecords_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id])
);
GO


CREATE TABLE [VitalSigns] (
    [Id] int NOT NULL IDENTITY,
    [AppointmentId] int NOT NULL,
    [Pulse] nvarchar(max) NULL,
    [Temperature] nvarchar(max) NULL,
    [BloodPressure] nvarchar(max) NULL,
    [SpO2] nvarchar(max) NULL,
    [NurseName] nvarchar(max) NULL,
    [RecordedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_VitalSigns] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_VitalSigns_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [LabTests] (
    [Id] int NOT NULL IDENTITY,
    [MedicalRecordId] int NOT NULL,
    [TestName] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [Result] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CompletedAt] datetime2 NULL,
    [ImageUrl] nvarchar(max) NULL,
    CONSTRAINT [PK_LabTests] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_LabTests_MedicalRecords_MedicalRecordId] FOREIGN KEY ([MedicalRecordId]) REFERENCES [MedicalRecords] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [Prescriptions] (
    [Id] int NOT NULL IDENTITY,
    [MedicalRecordId] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Prescriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Prescriptions_MedicalRecords_MedicalRecordId] FOREIGN KEY ([MedicalRecordId]) REFERENCES [MedicalRecords] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [PrescriptionDetails] (
    [Id] int NOT NULL IDENTITY,
    [PrescriptionId] int NOT NULL,
    [MedicineName] nvarchar(max) NOT NULL,
    [Quantity] int NOT NULL,
    [Unit] nvarchar(max) NOT NULL,
    [DosageInstruction] nvarchar(max) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_PrescriptionDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_PrescriptionDetails_Prescriptions_PrescriptionId] FOREIGN KEY ([PrescriptionId]) REFERENCES [Prescriptions] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_Appointments_PatientId] ON [Appointments] ([PatientId]);
GO


CREATE INDEX [IX_DiagnosticImages_PatientId] ON [DiagnosticImages] ([PatientId]);
GO


CREATE INDEX [IX_InsuranceCards_PatientId] ON [InsuranceCards] ([PatientId]);
GO


CREATE INDEX [IX_LabTests_MedicalRecordId] ON [LabTests] ([MedicalRecordId]);
GO


CREATE INDEX [IX_MedicalRecords_AppointmentId] ON [MedicalRecords] ([AppointmentId]);
GO


CREATE INDEX [IX_MedicalRecords_DepartmentId] ON [MedicalRecords] ([DepartmentId]);
GO


CREATE INDEX [IX_PrescriptionDetails_PrescriptionId] ON [PrescriptionDetails] ([PrescriptionId]);
GO


CREATE INDEX [IX_Prescriptions_MedicalRecordId] ON [Prescriptions] ([MedicalRecordId]);
GO


CREATE INDEX [IX_Receptions_PatientId] ON [Receptions] ([PatientId]);
GO


CREATE INDEX [IX_Surgeries_PatientId] ON [Surgeries] ([PatientId]);
GO


CREATE INDEX [IX_VitalSigns_AppointmentId] ON [VitalSigns] ([AppointmentId]);
GO


CREATE INDEX [IX_WorkSchedules_UserId] ON [WorkSchedules] ([UserId]);
GO


