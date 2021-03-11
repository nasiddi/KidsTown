USE CheckInsExtension;

IF OBJECT_ID('[cie].[LocationGroup]') IS NULL
BEGIN
CREATE TABLE [cie].[LocationGroup](
    [Id] [int] IDENTITY(1, 1) NOT NULL,
    [Name] varchar(50) NOT NULL,
    [IsEnabled] [bit] NOT NULL
    ) 
END;

IF NOT EXISTS( SELECT 1 FROM [cie].[LocationGroup] WHERE Name In (N'Häsli', N'Schöfli', N'Füchsli', 'Kids Church', 'Unbekannt'))
    BEGIN
        INSERT INTO [cie].[LocationGroup] (Name, IsEnabled)
        VALUES
        (N'Häsli', 1),
        (N'Schöfli', 1),
        (N'Füchsli', 1),
        ('Kids Church', 1),
        ('Unbekannt', 1)
    END;

IF OBJECT_ID('cie.[PK_LocationGroup]', 'PK') IS NULL
BEGIN
ALTER TABLE [cie].[LocationGroup] ADD CONSTRAINT [PK_LocationGroup] PRIMARY KEY CLUSTERED ( [Id] ASC )
    WITH (DATA_COMPRESSION=ROW)
END;

IF COL_LENGTH('cie.Location','LocationGroupId') IS NULL
BEGIN
    ALTER TABLE cie.Location ADD LocationGroupId int NOT NULL default 5
END

IF COL_LENGTH('cie.Location','CheckInsLocationId') IS NULL
    BEGIN
        ALTER TABLE cie.Location ADD CheckInsLocationId bigint NULL
    END

IF COL_LENGTH('cie.Location','EventId') IS NULL
    BEGIN
        ALTER TABLE cie.Location ADD EventId bigint NOT NULL DEFAULT 0
    END

IF OBJECT_ID('[cie].[FK_Location_LocationGroupId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [cie].[Location] ADD CONSTRAINT [FK_Location_LocationGroupId]
            FOREIGN KEY ([LocationGroupId])
                REFERENCES [cie].[LocationGroup] ([Id])
    END;
    


