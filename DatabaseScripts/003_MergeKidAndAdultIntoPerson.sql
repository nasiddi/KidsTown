use [KidsTown]

IF OBJECT_ID('[kt].[PersonType]') IS NULL
    BEGIN
        CREATE TABLE [kt].[PersonType](
                                          [Id] [int] IDENTITY(1, 1) NOT NULL,
                                          [Name] varchar(20) NOT NULL,
        )
    END;

IF NOT EXISTS( SELECT 1 FROM [kt].[PersonType] WHERE Name In ('Kid', 'Adult'))
    BEGIN
        INSERT INTO [kt].[PersonType] (Name)
        VALUES
        ('Kid'),
        ('Adult')
    END;
    

IF OBJECT_ID('[kt].[Person]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Person](
                                   [Id] [int] IDENTITY(1, 1) NOT NULL,
                                   [FamilyId] [int] NULL,
                                   [PeopleId] [bigint] NULL,
                                   [PersonTypeId] [int] NOT NULL,
                                   [FirstName] varchar(50) NOT NULL,
                                   [LastName] varchar(50) NOT NULL,
                                   [UpdateDate] datetime2 NOT NULL,
                                   CONSTRAINT [PK_Person_Alt] unique (Id,PersonTypeID),
                                   CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED ( [Id] ASC ) WITH (DATA_COMPRESSION=ROW)
        )
    END;

IF OBJECT_ID('[kt].[Kid]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Kid](
                                   [PersonId] [int] NOT NULL,
                                   [PersonTypeId] as 1 persisted,
                                   [MayLeaveAlone] bit NOT NULL,
                                   [HasPeopleWithoutPickupPermission] bit NOT NULL,
                                   [UpdateDate] datetime2 NOT NULL
        )
    END;

IF OBJECT_ID('[kt].[Adult]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Adult](
                                  [PersonId] [int] NOT NULL,
                                  [PersonTypeId] as 2 persisted,
                                  [PhoneNumber] varchar(30) NOT NULL,
                                  [UpdateDate] datetime2 NOT NULL
        )
    END;

IF OBJECT_ID('kt.[PK_PersonType]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[PersonType] ADD CONSTRAINT [PK_PersonType] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;    

IF OBJECT_ID('kt.[PK_Kid]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Kid] ADD CONSTRAINT [PK_Kid] PRIMARY KEY CLUSTERED ( [PersonId] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_Adult]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Adult] ADD CONSTRAINT [PK_Adult] PRIMARY KEY CLUSTERED ( [PersonId] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_Person_PersonTypeId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Person] ADD CONSTRAINT [FK_Person_PersonTypeId]
            FOREIGN KEY ([PersonTypeId])
                REFERENCES [kt].[PersonType] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_Kid_PersonId_PersonTypeId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Kid] ADD CONSTRAINT [FK_Kid_PersonId_PersonTypeId]
            FOREIGN KEY ([PersonId], [PersonTypeId])
                REFERENCES [kt].[Person] ([Id], [PersonTypeId])
    END;

IF OBJECT_ID('[kt].[FK_Adult_PersonId_PersonTypeId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Adult] ADD CONSTRAINT [FK_Adult_PersonId_PersonTypeId]
            FOREIGN KEY ([PersonId], [PersonTypeId])
                REFERENCES [kt].[Person] ([Id], [PersonTypeId])
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.Kid'), 'XI_Kid_PersonId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [XI_Kid_PersonId] ON [kt].[Kid]
            (
             [PersonId], [PersonTypeId] ASC
                )
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=OFF)
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.Adult'), 'XI_Adult_PersonId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [XI_Adult_PersonId] ON [kt].[Adult]
            (
             [PersonId], [PersonTypeId] ASC
                )
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=OFF)
    END;
    