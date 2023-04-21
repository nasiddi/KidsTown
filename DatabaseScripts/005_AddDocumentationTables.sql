use [KidsTown]

IF OBJECT_ID('[kt].[DocElement]') IS NULL
    BEGIN
        CREATE TABLE [kt].[DocElement](
                                        [Id] [int] NOT NULL,
                                        [PreviousId] [int] NULL,
                                        [SectionId] [int] NOT NULL,
                                        [UpdateDate] [DATETIME2] NOT NULL)
    END;

IF OBJECT_ID('kt.[PK_DocElement]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocElement] ADD CONSTRAINT [PK_DocElement] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_DocElement_PreviousId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocElement] ADD CONSTRAINT [FK_DocElement_PreviousId]
            FOREIGN KEY ([PreviousId])
                REFERENCES [kt].[DocElement] ([Id])
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.DocElement'), 'UQ_DocElement_PreviousId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_DocElement_PreviousId] ON [kt].[DocElement]
            (
             [PreviousId] ASC
                )
            WHERE PreviousId is not null
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[DocTitle]') IS NULL
    BEGIN
        CREATE TABLE [kt].[DocTitle](
                                          [Id] [int] IDENTITY(1, 1) NOT NULL,
                                          [ElementId] [int] NOT NULL,
                                          [Content] nvarchar(100) NOT NULL,
                                          [Size] [int] NOT NULL)
    END;

IF OBJECT_ID('kt.[PK_DocTitle]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocTitle] ADD CONSTRAINT [PK_DocTitle] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_DocTitle_ElementId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocTitle] ADD CONSTRAINT [FK_DocTitle_ElementId]
            FOREIGN KEY ([ElementId])
                REFERENCES [kt].[DocElement] ([Id])
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.DocTitle'), 'UQ_DocTitle_ElementId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_DocTitle_ElementId] ON [kt].[DocTitle]
            (
             [ElementId] ASC
                )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[DocParagraph]') IS NULL
    BEGIN
        CREATE TABLE [kt].[DocParagraph](
                                        [Id] [int] NOT NULL,
                                        [PreviousId] [int] NULL,
                                        [ElementId] [int] NOT NULL,
                                        [Content] nvarchar(512) NOT NULL,
                                        [ParagraphIconId] [int] NULL)
    END;

IF OBJECT_ID('kt.[PK_DocParagraph]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocParagraph] ADD CONSTRAINT [PK_DocParagraph] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_DocParagraph_PreviousId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocParagraph] ADD CONSTRAINT [FK_DocParagraph_PreviousId]
            FOREIGN KEY ([PreviousId])
                REFERENCES [kt].[DocParagraph] ([Id])
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.DocParagraph'), 'UQ_DocParagraph_PreviousId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_DocParagraph_PreviousId] ON [kt].[DocParagraph]
            (
             [PreviousId] ASC
                )
            WHERE PreviousId is not null
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_DocParagraph_ElementId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocParagraph] ADD CONSTRAINT [FK_DocParagraph_ElementId]
            FOREIGN KEY ([ElementId])
                REFERENCES [kt].[DocElement] ([Id])
    END;

IF OBJECT_ID('[kt].[DocImage]') IS NULL
BEGIN
CREATE TABLE [kt].[DocImage](
    [Id] [int] NOT NULL,
    [PreviousId] [int] NULL,
    [ElementId] [int] NOT NULL,
    [FileId] nvarchar(100) NOT NULL)
END;

IF OBJECT_ID('kt.[PK_DocImage]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocImage] ADD CONSTRAINT [PK_DocImage] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_DocImage_PreviousId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocImage] ADD CONSTRAINT [FK_DocImage_PreviousId]
            FOREIGN KEY ([PreviousId])
                REFERENCES [kt].[DocImage] ([Id])
    END;

IF INDEXPROPERTY(OBJECT_ID('kt.DocImage'), 'UQ_DocImage_PreviousId', 'IndexId') IS NULL
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX [UQ_DocImage_PreviousId] ON [kt].[DocImage]
            (
             [PreviousId] ASC
                )
            WHERE PreviousId is not null
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_DocImage_ElementId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[DocImage] ADD CONSTRAINT [FK_DocImage_ElementId]
            FOREIGN KEY ([ElementId])
                REFERENCES [kt].[DocElement] ([Id])
    END;