use [KidsTown]

IF OBJECT_ID('[kt].[Parent]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Parent](
                                      [Id] [int] IDENTITY(1, 1) NOT NULL,
                                      [PeopleId] [bigint] NOT NULL,
                                      [FamilyId] [int] NOT NULL,
                                      [FistName] varchar(50) NOT NULL,
                                      [LastName] varchar(50) NOT NULL,
                                      [PhoneNumber] varchar(30) NULL,
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

IF COL_LENGTH('kt.Person','FamilyId') IS NULL
    BEGIN
        ALTER TABLE kt.Person ADD FamilyId [int] NULL
    END

IF OBJECT_ID('kt.[PK_Parent]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Parent] ADD CONSTRAINT [PK_Parent] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_Family]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[Family] ADD CONSTRAINT [PK_Family] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_Parent_FamilyId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Parent] ADD CONSTRAINT [FK_Parent_FamilyId]
            FOREIGN KEY ([FamilyId])
                REFERENCES [kt].[Family] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_Person_FamilyId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Person] ADD CONSTRAINT [FK_Person_FamilyId]
            FOREIGN KEY ([FamilyId])
                REFERENCES [kt].[Family] ([Id])
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.Parent'), 'UQ_Parent_PeopleId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_Parent_PeopleId] ON [kt].[Parent]
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


