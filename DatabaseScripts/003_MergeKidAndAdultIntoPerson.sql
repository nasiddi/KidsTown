use [KidsTown]    

IF OBJECT_ID('[kt].[Person]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Person](
                                   [Id] [int] IDENTITY(1, 1) NOT NULL,
                                   [FamilyId] [int] NULL,
                                   [PeopleId] [bigint] NULL,
                                   [FirstName] varchar(50) NOT NULL,
                                   [LastName] varchar(50) NOT NULL,
                                   [UpdateDate] datetime2 NOT NULL,
        )
    END;

IF OBJECT_ID('[kt].[Kid]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Kid](
                                   [PersonId] [int] NOT NULL,
                                   [MayLeaveAlone] bit NOT NULL,
                                   [HasPeopleWithoutPickupPermission] bit NOT NULL,
        )
    END;

IF OBJECT_ID('[kt].[Adult]') IS NULL
    BEGIN
        CREATE TABLE [kt].[Adult](
                                  [PersonId] [int] NOT NULL,
                                  [PhoneNumber] varchar(30) NOT NULL,
        )
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

IF OBJECT_ID('[kt].[FK_Kid_PersonId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Kid] ADD CONSTRAINT [FK_Kid_PersonId]
            FOREIGN KEY ([PersonId])
                REFERENCES [kt].[Person] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_Adult_PersonId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[Adult] ADD CONSTRAINT [FK_Adult_PersonId]
            FOREIGN KEY ([PersonId])
                REFERENCES [kt].[Person] ([Id])
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.Kid'), 'XI_Kid_PersonId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [XI_Kid_PersonId] ON [kt].[Kid]
            (
             [PersonId] ASC
                )
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=OFF)
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.Adult'), 'XI_Adult_PersonId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [XI_Adult_PersonId] ON [kt].[Adult]
            (
             [PersonId] ASC
                )
            WITH (DATA_COMPRESSION=ROW, SORT_IN_TEMPDB=ON, ONLINE=OFF)
    END;
    