USE CheckInsExtension;

IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'cie' )
    EXEC('CREATE SCHEMA [cie]');
GO

IF OBJECT_ID('[cie].[Attendance]') IS NULL
    BEGIN
        CREATE TABLE [cie].[Attendance](
                                        [Id] [int] IDENTITY(1, 1) NOT NULL,
                                        [CheckInId] [bigint] NOT NULL,
                                        [PersonId] [int] NOT NULL,
                                        [LocationId] [int] NOT NULL,
                                        [SecurityCode] varchar(10) NOT NULL,
                                        [AttendanceTypeId] [int] NOT NULL,
                                        [InsertDate] DATETIME2 NOT NULL,
                                        [CheckInDate] DATETIME2 NULL,
                                        [CheckOutDate] DATETIME2 NULL
        )
    END;

IF OBJECT_ID('[cie].[Person]') IS NULL
    BEGIN
        CREATE TABLE [cie].[Person](
                                       [Id] [int] IDENTITY(1, 1) NOT NULL,
                                       [PeopleId] [bigint] NULL,
                                       [FistName] varchar(50) NOT NULL,
                                       [LastName] varchar(50) NOT NULL,
                                       [MayLeaveAlone] bit NULL,
                                       [HasPeopleWithoutPickupPermission] bit NULL
        )
    END;

IF OBJECT_ID('[cie].[AttendanceType]') IS NULL
    BEGIN
        CREATE TABLE [cie].[AttendanceType](
                                             [Id] [int] IDENTITY(1, 1) NOT NULL,
                                             [Name] varchar(50) NOT NULL,
        )
    END;

IF OBJECT_ID('[cie].[Location]') IS NULL
    BEGIN
        CREATE TABLE [cie].[Location](
                                         [Id] [int] IDENTITY(1, 1) NOT NULL,
                                         [Name] varchar(50) NOT NULL,
                                         [IsEnabled] [bit] NOT NULL
        )
    END;



IF OBJECT_ID('cie.[PK_Attendance]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [cie].[Attendance] ADD CONSTRAINT [PK_Attendance] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('cie.[PK_Person]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [cie].[Person] ADD CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('cie.[PK_AttendanceType]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [cie].[AttendanceType] ADD CONSTRAINT [PK_AttendanceType] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('cie.[PK_Location]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [cie].[Location] ADD CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[cie].[FK_Attendance_PersonId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [cie].[Attendance] ADD CONSTRAINT [FK_Attendance_PersonId]
            FOREIGN KEY ([PersonId])
                REFERENCES [cie].[Person] ([Id])
    END;

IF OBJECT_ID('[cie].[FK_Attendance_AttendanceTypeId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [cie].[Attendance] ADD CONSTRAINT [FK_Attendance_AttendanceTypeId]
            FOREIGN KEY ([AttendanceTypeId])
                REFERENCES [cie].[AttendanceType] ([Id])
    END;

IF OBJECT_ID('[cie].[FK_Attendance_LocationId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [cie].[Attendance] ADD CONSTRAINT [FK_Attendance_LocationId]
            FOREIGN KEY ([LocationId])
                REFERENCES [cie].[Location] ([Id])
    END;

IF NOT EXISTS( SELECT 1 FROM [cie].[Location] WHERE Name In (N'Häsli', N'Schöfli', N'Füchsli', 'KidsChurch'))
    BEGIN
        INSERT INTO [cie].[Location] (Name, IsEnabled)
        VALUES
        (N'Häsli', 1),
        (N'Schöfli', 1),
        (N'Füchsli', 1),
        ('KidsChurch', 1)
    END;

SET IDENTITY_INSERT cie.AttendanceType ON

IF NOT EXISTS( SELECT 1 FROM [cie].[AttendanceType] WHERE Id = 1)
    BEGIN
        INSERT INTO [cie].[AttendanceType] (Id, Name)
        VALUES
        (1, 'Regular')
    END;

IF NOT EXISTS( SELECT 1 FROM [cie].[AttendanceType] WHERE Id = 2)
    BEGIN
        INSERT INTO [cie].[AttendanceType] (Id, Name)
        VALUES
        (2, 'Guest')
    END;

IF NOT EXISTS( SELECT 1 FROM [cie].[AttendanceType] WHERE Id = 3)
    BEGIN
        INSERT INTO [cie].[AttendanceType] (Id, Name)
        VALUES
        (3, 'Volunteer')
    END;

SET IDENTITY_INSERT cie.AttendanceType OFF

IF INDEXPROPERTY(OBJECT_ID('cie.Person'), 'UQ_Person_PeopleId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_Person_PeopleId] ON [cie].[Person]
            (
             [PeopleId] ASC
                )
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=OFF)
    END;


-- dotnet ef dbcontext scaffold "Server=127.0.0.1,1401;Database=CheckInExtension;User Id=sa;Password=Sherlock69" Microsoft.EntityFrameworkCore.SqlServer -f
    

    

