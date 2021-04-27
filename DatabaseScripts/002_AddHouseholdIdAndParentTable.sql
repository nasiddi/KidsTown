use [KidsTown]

IF OBJECT_ID('[kt].[Adult]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Adult](
                                      [Id] [int] IDENTITY(1, 1) NOT NULL,
                                      [PeopleId] [bigint] NOT NULL,
                                      [FamilyId] [int] NOT NULL,
                                      [FirstName] varchar(50) NOT NULL,
                                      [LastName] varchar(50) NOT NULL,
                                      [PhoneNumber] varchar(30) NOT NULL,
                                      [UpdateDate] datetime2 NOT NULL
        )
    END;

IF OBJECT_ID('[kt].[Family]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Family](
                                      [Id] [int] IDENTITY(1, 1) NOT NULL,
                                      [HouseholdId] [bigint] NOT NULL,
                                      [Name] varchar(70) NOT NULL
                                    
        )
    END;

IF COL_LENGTH('kt.Kid','FamilyId') IS NULL
    BEGIN
        ALTER TABLE kt.Kid ADD FamilyId [int] NULL
    END

IF OBJECT_ID('kt.[PK_Adult]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Adult] ADD CONSTRAINT [PK_Adult] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_Family]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Family] ADD CONSTRAINT [PK_Family] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_Adult_FamilyId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Adult] ADD CONSTRAINT [FK_Adult_FamilyId]
            FOREIGN KEY ([FamilyId])
                REFERENCES [kt].[Family] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_Kid_FamilyId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Kid] ADD CONSTRAINT [FK_Kid_FamilyId]
            FOREIGN KEY ([FamilyId])
                REFERENCES [kt].[Family] ([Id])
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.Adult'), 'UQ_Adult_PeopleId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_Adult_PeopleId] ON [kt].[Adult]
            (
             [PeopleId] ASC
                )
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=OFF)
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.Family'), 'UQ_Family_HouseholdId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_Family_HouseholdId] ON [kt].[Family]
            (
             [HouseholdId] ASC
                )
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=OFF)
    END;


