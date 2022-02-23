use [KidsTown]

IF OBJECT_ID('[kt].[DocumentationTab]') IS NULL
    BEGIN
        CREATE TABLE [kt].[DocumentationTab](
                                                [Id] [int] IDENTITY(1, 1) NOT NULL,
                                                [Name] varchar(50) NULL,
        )
    END;

IF OBJECT_ID('[kt].[DocumentationElement]') IS NULL
    BEGIN
        CREATE TABLE [kt].[DocumentationElement](
                                                    [Id] [int] IDENTITY(1, 1) NOT NULL,
                                                    [Position] [int] NOT NULL,
                                                    [DocumentationTabId] [int] NOT NULL
        )
    END;

IF OBJECT_ID('[kt].[DocumentationTitle]') IS NULL
    BEGIN
        CREATE TABLE [kt].[DocumentationTitle](
                                                  [Id] [int] IDENTITY(1, 1) NOT NULL,
                                                  [DocumentElementId] [int] NOT NULL,
                                                  [Size] [int] NOT NULL,
                                                  [Text] varchar(200) NOT NULL
        )
    END;

IF OBJECT_ID('[kt].[DocumentationEntry]') IS NULL
    BEGIN
        CREATE TABLE [kt].[DocumentationEntry](
                                                  [Id] [int] IDENTITY(1, 1) NOT NULL,
                                                  [DocumentElementId] [int] NOT NULL,
                                                  [FileName] varchar(100) NULL,


        )
    END;

IF OBJECT_ID('[kt].[DocumentEntryParagraph]') IS NULL
    BEGIN
        CREATE TABLE [kt].[DocumentEntryParagraph](
                                                      [Id] [int] IDENTITY(1, 1) NOT NULL,
                                                      [DocumentationEntryId] [int] NOT NULL,
                                                      [Position] [int] NOT NULL,
                                                      [Text] varchar(max) NOT NULL,
                                                      [Icon] varchar(50) NULL,

        )
    END;

IF OBJECT_ID('kt.[PK_DocumentationElement]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentationElement] ADD CONSTRAINT [PK_DocumentationElement] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[DocumentationTab]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentationTab] ADD CONSTRAINT [PK_DocumentationTab] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_DocumentationTitle]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentationTitle] ADD CONSTRAINT [PK_DocumentationTitle] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_DocumentationEntry]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentationEntry] ADD CONSTRAINT [PK_DocumentationEntry] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_DocumentEntryParagraph]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentEntryParagraph] ADD CONSTRAINT [PK_DocumentEntryParagraph] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_DocumentationTitle_DocumentationElementId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentationTitle] ADD CONSTRAINT [FK_DocumentationTitle_DocumentationElementId]
            FOREIGN KEY ([DocumentElementId])
                REFERENCES [kt].[DocumentationElement] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_DocumentationEntry_DocumentationElementId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentationEntry] ADD CONSTRAINT [FK_DocumentationEntry_DocumentationElementId]
            FOREIGN KEY ([DocumentElementId])
                REFERENCES [kt].[DocumentationElement] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_DocumentEntryParagraph_DocumentationEntryId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentEntryParagraph] ADD CONSTRAINT [FK_DocumentEntryParagraph_DocumentationEntryId]
            FOREIGN KEY ([DocumentationEntryId])
                REFERENCES [kt].[DocumentationEntry] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_DocumentationElement_DocumentationTabId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocumentationElement] ADD CONSTRAINT [FK_DocumentationElement_DocumentationTabId]
            FOREIGN KEY ([DocumentationTabId])
                REFERENCES [kt].[DocumentationTab] ([Id])
    END;
    
    