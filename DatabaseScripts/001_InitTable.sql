IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'KidsTown')
    BEGIN
        CREATE DATABASE [KidsTown]
    END 

use [KidsTown]

IF NOT EXISTS ( SELECT  *
                FROM    sys.schemas
                WHERE   name = N'kt' )
    EXEC('CREATE SCHEMA [kt]');
GO

IF OBJECT_ID('[kt].[Attendance]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Attendance](
                                        [Id] [int] IDENTITY(1, 1) NOT NULL,
                                        [CheckInsId] [bigint] NOT NULL,
                                        [KidId] [int] NOT NULL,
                                        [LocationId] [int] NOT NULL,
                                        [SecurityCode] varchar(10) NOT NULL,
                                        [AttendanceTypeId] [int] NOT NULL,
                                        [InsertDate] DATETIME2 NOT NULL,
                                        [CheckInDate] DATETIME2 NULL,
                                        [CheckOutDate] DATETIME2 NULL
        )
    END;

IF OBJECT_ID('[kt].[Kid]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Kid](
                                       [Id] [int] IDENTITY(1, 1) NOT NULL,
                                       [PeopleId] [bigint] NULL,
                                       [FirstName] varchar(50) NOT NULL,
                                       [LastName] varchar(50) NOT NULL,
                                       [MayLeaveAlone] bit NOT NULL,
                                       [HasPeopleWithoutPickupPermission] bit NOT NULL
        )
    END;

IF OBJECT_ID('[kt].[AttendanceType]') IS NULL
    BEGIN
        CREATE TABLE [kt].[AttendanceType](
                                             [Id] [int] IDENTITY(1, 1) NOT NULL,
                                             [Name] varchar(50) NOT NULL,
        )
    END;

IF OBJECT_ID('[kt].[Location]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Location](
                                         [Id] [int] IDENTITY(1, 1) NOT NULL,
                                         [Name] varchar(50) NOT NULL,
        )
    END;


IF OBJECT_ID('[kt].[TaskExecution]') IS NULL
    BEGIN
        CREATE TABLE [kt].[TaskExecution](
                                        [Id] [int] IDENTITY(1, 1) NOT NULL,
                                        [InsertDate] DATETIME2 NOT NULL,
                                        [IsSuccess] [bit] NOT NULL,
                                        [UpdateCount] [int] NOT NULL,
                                        [Environment] varchar(20) NOT NULL,
        )
    END;



IF OBJECT_ID('kt.[PK_Attendance]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Attendance] ADD CONSTRAINT [PK_Attendance] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_Kid]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Kid] ADD CONSTRAINT [PK_Kid] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_AttendanceType]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[AttendanceType] ADD CONSTRAINT [PK_AttendanceType] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_Location]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Location] ADD CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_TaskExecution]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[TaskExecution] ADD CONSTRAINT [PK_TaskExecution] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_Attendance_PersonId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Attendance] ADD CONSTRAINT [FK_Attendance_PersonId]
            FOREIGN KEY ([PersonId])
                REFERENCES [kt].[Person] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_Attendance_AttendanceTypeId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Attendance] ADD CONSTRAINT [FK_Attendance_AttendanceTypeId]
            FOREIGN KEY ([AttendanceTypeId])
                REFERENCES [kt].[AttendanceType] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_Attendance_LocationId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Attendance] ADD CONSTRAINT [FK_Attendance_LocationId]
            FOREIGN KEY ([LocationId])
                REFERENCES [kt].[Location] ([Id])
    END;

SET IDENTITY_INSERT kt.AttendanceType ON

IF NOT EXISTS( SELECT 1 FROM [kt].[AttendanceType] WHERE Id = 1)
    BEGIN
        INSERT INTO [kt].[AttendanceType] (Id, Name)
        VALUES
        (1, 'Regular')
    END;

IF NOT EXISTS( SELECT 1 FROM [kt].[AttendanceType] WHERE Id = 2)
    BEGIN
        INSERT INTO [kt].[AttendanceType] (Id, Name)
        VALUES
        (2, 'Guest')
    END;

IF NOT EXISTS( SELECT 1 FROM [kt].[AttendanceType] WHERE Id = 3)
    BEGIN
        INSERT INTO [kt].[AttendanceType] (Id, Name)
        VALUES
        (3, 'Volunteer')
    END;

SET IDENTITY_INSERT kt.AttendanceType OFF

IF INDEXPROPERTY(OBJECT_ID('kt.Kid'), 'UQ_Kid_PeopleId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_Kid_PeopleId] ON [kt].[Kid]
            (
             [PeopleId] ASC
                )
            WHERE PeopleId IS NOT NULL
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=OFF)
    END;

IF OBJECT_ID('[kt].[LocationGroup]') IS NULL
    BEGIN
        CREATE TABLE [kt].[LocationGroup](
                                             [Id] [int] IDENTITY(1, 1) NOT NULL,
                                             [Name] varchar(50) NOT NULL,
                                             [IsEnabled] [bit] NOT NULL
        )
    END;

IF NOT EXISTS( SELECT 1 FROM [kt].[LocationGroup] WHERE Name In (N'Häsli', N'Schöfli', N'Füchsli', 'Kids Church', 'Unbekannt'))
    BEGIN
        INSERT INTO [kt].[LocationGroup] (Name, IsEnabled)
        VALUES
        (N'Häsli', 1),
        (N'Schöfli', 1),
        (N'Füchsli', 1),
        ('Kids Church', 1),
        ('Unbekannt', 1)
    END;

IF OBJECT_ID('kt.[PK_LocationGroup]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[LocationGroup] ADD CONSTRAINT [PK_LocationGroup] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF COL_LENGTH('kt.Location','LocationGroupId') IS NULL
    BEGIN
        ALTER TABLE kt.Location ADD LocationGroupId int NOT NULL default 5
    END

IF COL_LENGTH('kt.Location','CheckInsLocationId') IS NULL
    BEGIN
        ALTER TABLE kt.Location ADD CheckInsLocationId bigint NULL
    END

IF COL_LENGTH('kt.Location','EventId') IS NULL
    BEGIN
        ALTER TABLE kt.Location ADD EventId bigint NOT NULL DEFAULT 0
    END

IF OBJECT_ID('[kt].[FK_Location_LocationGroupId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Location] ADD CONSTRAINT [FK_Location_LocationGroupId]
            FOREIGN KEY ([LocationGroupId])
                REFERENCES [kt].[LocationGroup] ([Id])
    END;
    

    

