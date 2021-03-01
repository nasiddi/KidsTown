IF OBJECT_ID('[cie].[CheckIn]') IS NULL
    BEGIN
        CREATE TABLE [cie].[CheckIn](
                                        [Id] [int] IDENTITY(1, 1) NOT NULL,
                                        [CheckInId] [bigint] NOT NULL,
                                        [PersonId] [int] NOT NULL,
                                        [LocationId] [int] NOT NULL,
                                        [SecurityCode] varchar(10) NOT NULL,
                                        [AttendeeTypeId] [int] NOT NULL,
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

IF OBJECT_ID('[cie].[AttendeeType]') IS NULL
    BEGIN
        CREATE TABLE [cie].[AttendeeType](
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



IF OBJECT_ID('cie.[PK_Checkin]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [cie].[CheckIn] ADD CONSTRAINT [PK_Checkin] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('cie.[PK_Person]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [cie].[Person] ADD CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('cie.[PK_AttendeeType]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [cie].[AttendeeType] ADD CONSTRAINT [PK_AttendeeType] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('cie.[PK_Location]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [cie].[Location] ADD CONSTRAINT [PK_Location] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[cie].[FK_Checkin_PersonId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [cie].[CheckIn] ADD CONSTRAINT [FK_Checkin_PersonId]
            FOREIGN KEY ([PersonId])
                REFERENCES [cie].[Person] ([Id])
    END;

IF OBJECT_ID('[cie].[FK_CheckIn_AttendeeTypeId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [cie].[CheckIn] ADD CONSTRAINT [FK_CheckIn_AttendeeTypeId]
            FOREIGN KEY ([AttendeeTypeId])
                REFERENCES [cie].[AttendeeType] ([Id])
    END;

IF OBJECT_ID('[cie].[FK_CheckIn_LocationId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [cie].[CheckIn] ADD CONSTRAINT [FK_CheckIn_LocationId]
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

SET IDENTITY_INSERT cie.AttendeeType ON

IF NOT EXISTS( SELECT 1 FROM [cie].[AttendeeType] WHERE Id = 1)
    BEGIN
        INSERT INTO [cie].[AttendeeType] (Id, Name)
        VALUES
        (1, 'Regular')
    END;

IF NOT EXISTS( SELECT 1 FROM [cie].[AttendeeType] WHERE Id = 2)
    BEGIN
        INSERT INTO [cie].[AttendeeType] (Id, Name)
        VALUES
        (2, 'Guest')
    END;

IF NOT EXISTS( SELECT 1 FROM [cie].[AttendeeType] WHERE Id = 3)
    BEGIN
        INSERT INTO [cie].[AttendeeType] (Id, Name)
        VALUES
        (3, 'Volunteer')
    END;

SET IDENTITY_INSERT cie.AttendeeType OFF

IF INDEXPROPERTY(OBJECT_ID('cie.Person'), 'UQ_Person_PeopleId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_Person_PeopleId] ON [cie].[Person]
            (
             [PeopleId] ASC
                )
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=ON)
    END;


-- dotnet ef dbcontext scaffold "Server=127.0.0.1,1401;Database=CheckInExtension;User Id=sa;Password=Sherlock69" Microsoft.EntityFrameworkCore.SqlServer -f
    

    

