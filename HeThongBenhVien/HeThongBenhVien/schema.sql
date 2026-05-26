IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE TABLE [MedicineUnits] (
        [Id] int NOT NULL IDENTITY,
        [UnitName] nvarchar(50) NOT NULL,
        [DefaultPrice] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_MedicineUnits] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE TABLE [Patients] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(100) NOT NULL,
        [Gender] nvarchar(max) NOT NULL,
        [Age] int NOT NULL,
        [PatientCode] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_Patients] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE TABLE [Appointments] (
        [Id] int NOT NULL IDENTITY,
        [PatientId] int NOT NULL,
        [Reason] nvarchar(200) NOT NULL,
        [AppointmentTime] datetime2 NOT NULL,
        [Status] int NOT NULL,
        CONSTRAINT [PK_Appointments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Appointments_Patients_PatientId] FOREIGN KEY ([PatientId]) REFERENCES [Patients] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE TABLE [MedicalRecords] (
        [Id] int NOT NULL IDENTITY,
        [AppointmentId] int NOT NULL,
        [Symptoms] nvarchar(max) NOT NULL,
        [Vitals] nvarchar(max) NOT NULL,
        [Diagnosis] nvarchar(max) NOT NULL,
        [TreatmentPlan] nvarchar(max) NOT NULL,
        [Notes] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MedicalRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MedicalRecords_Appointments_AppointmentId] FOREIGN KEY ([AppointmentId]) REFERENCES [Appointments] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE TABLE [LabTests] (
        [Id] int NOT NULL IDENTITY,
        [MedicalRecordId] int NOT NULL,
        [TestName] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [Result] nvarchar(max) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CompletedAt] datetime2 NULL,
        CONSTRAINT [PK_LabTests] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_LabTests_MedicalRecords_MedicalRecordId] FOREIGN KEY ([MedicalRecordId]) REFERENCES [MedicalRecords] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE TABLE [Prescriptions] (
        [Id] int NOT NULL IDENTITY,
        [MedicalRecordId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Prescriptions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Prescriptions_MedicalRecords_MedicalRecordId] FOREIGN KEY ([MedicalRecordId]) REFERENCES [MedicalRecords] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Appointments_PatientId] ON [Appointments] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_LabTests_MedicalRecordId] ON [LabTests] ([MedicalRecordId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_MedicalRecords_AppointmentId] ON [MedicalRecords] ([AppointmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PrescriptionDetails_PrescriptionId] ON [PrescriptionDetails] ([PrescriptionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Prescriptions_MedicalRecordId] ON [Prescriptions] ([MedicalRecordId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260510141003_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260510141003_InitialCreate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512064306_AddMedicalModules'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512064306_AddMedicalModules'
)
BEGIN
    CREATE TABLE [MedicalServices] (
        [Id] int NOT NULL IDENTITY,
        [ServiceCode] nvarchar(max) NOT NULL,
        [ServiceName] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_MedicalServices] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512064306_AddMedicalModules'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260512064306_AddMedicalModules', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512071233_AddMedicineModel'
)
BEGIN
    CREATE TABLE [Medicines] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Price] decimal(18,2) NOT NULL,
        CONSTRAINT [PK_Medicines] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512071233_AddMedicineModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260512071233_AddMedicineModel', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512072916_AddPatientCodeToUser'
)
BEGIN
    ALTER TABLE [Users] ADD [PatientCode] nvarchar(20) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512072916_AddPatientCodeToUser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260512072916_AddPatientCodeToUser', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512105202_AddVitalSignsTable'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512105202_AddVitalSignsTable'
)
BEGIN
    CREATE INDEX [IX_VitalSigns_AppointmentId] ON [VitalSigns] ([AppointmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260512105202_AddVitalSignsTable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260512105202_AddVitalSignsTable', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    ALTER TABLE [Medicines] ADD [Category] nvarchar(100) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    ALTER TABLE [Medicines] ADD [ExpiryDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    ALTER TABLE [Medicines] ADD [IsActive] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    ALTER TABLE [Medicines] ADD [Manufacturer] nvarchar(200) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    ALTER TABLE [Medicines] ADD [MinStock] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    ALTER TABLE [Medicines] ADD [StockQuantity] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    ALTER TABLE [Medicines] ADD [Unit] nvarchar(50) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    CREATE TABLE [Departments] (
        [Id] int NOT NULL IDENTITY,
        [DepartmentCode] nvarchar(50) NOT NULL,
        [DepartmentName] nvarchar(100) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [HeadDoctor] nvarchar(100) NOT NULL,
        [TotalBeds] int NOT NULL,
        [OccupiedBeds] int NOT NULL,
        [Phone] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Departments] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
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
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    CREATE INDEX [IX_DiagnosticImages_PatientId] ON [DiagnosticImages] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    CREATE INDEX [IX_InsuranceCards_PatientId] ON [InsuranceCards] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    CREATE INDEX [IX_Receptions_PatientId] ON [Receptions] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    CREATE INDEX [IX_Surgeries_PatientId] ON [Surgeries] ([PatientId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260514083138_AddMedicineColumns'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260514083138_AddMedicineColumns', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515032737_AddAdmissionFieldsToMedicalRecord'
)
BEGIN
    ALTER TABLE [MedicalRecords] ADD [AdmissionDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515032737_AddAdmissionFieldsToMedicalRecord'
)
BEGIN
    ALTER TABLE [MedicalRecords] ADD [DischargeDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515032737_AddAdmissionFieldsToMedicalRecord'
)
BEGIN
    ALTER TABLE [MedicalRecords] ADD [RoomFee] decimal(18,2) NOT NULL DEFAULT 0.0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260515032737_AddAdmissionFieldsToMedicalRecord'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260515032737_AddAdmissionFieldsToMedicalRecord', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260521094600_RemoveOldVitalsFromMedicalRecor'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MedicalRecords]') AND [c].[name] = N'Vitals');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [MedicalRecords] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [MedicalRecords] DROP COLUMN [Vitals];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260521094600_RemoveOldVitalsFromMedicalRecor'
)
BEGIN
    ALTER TABLE [MedicalRecords] ADD [BedNumber] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260521094600_RemoveOldVitalsFromMedicalRecor'
)
BEGIN
    ALTER TABLE [MedicalRecords] ADD [DepartmentId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260521094600_RemoveOldVitalsFromMedicalRecor'
)
BEGIN
    CREATE INDEX [IX_MedicalRecords_DepartmentId] ON [MedicalRecords] ([DepartmentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260521094600_RemoveOldVitalsFromMedicalRecor'
)
BEGIN
    ALTER TABLE [MedicalRecords] ADD CONSTRAINT [FK_MedicalRecords_Departments_DepartmentId] FOREIGN KEY ([DepartmentId]) REFERENCES [Departments] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260521094600_RemoveOldVitalsFromMedicalRecor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260521094600_RemoveOldVitalsFromMedicalRecor', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526032202_AddSurgeryFieldsAndMissingTables'
)
BEGIN
    ALTER TABLE [MedicalRecords] ADD [SurgeonId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526032202_AddSurgeryFieldsAndMissingTables'
)
BEGIN
    ALTER TABLE [MedicalRecords] ADD [SurgeryFeeId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526032202_AddSurgeryFieldsAndMissingTables'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Departments]') AND [c].[name] = N'Phone');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Departments] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Departments] ALTER COLUMN [Phone] nvarchar(10) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260526032202_AddSurgeryFieldsAndMissingTables'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260526032202_AddSurgeryFieldsAndMissingTables', N'8.0.0');
END;
GO

COMMIT;
GO

