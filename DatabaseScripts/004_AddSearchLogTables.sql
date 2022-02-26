use [KidsTown]

IF OBJECT_ID('[kt].[SearchLog]') IS NULL
    BEGIN
        CREATE TABLE [kt].[SearchLog](
                                      [Id] [int] IDENTITY(1, 1) NOT NULL,
                                      [SearchDate] DATETIME2 NOT NULL,
                                      [SecurityCode] varchar(10) NOT NULL,
                                      [DeviceGuid] varchar(40) NOT NULL,
                                      [IsCheckIn] bit NOT NULL,
                                      [EventId] bigint NOT NULL
        )
    END;

IF OBJECT_ID('[kt].[SearchLog2LocationGroup]') IS NULL
    BEGIN
        CREATE TABLE [kt].[SearchLog2LocationGroup](
                                      [Id] [int] IDENTITY(1, 1) NOT NULL,
                                      [SearchLogId] [int] NOT NULL,
                                      [LocationGroupId] int NOT NULL
        )
    END;

IF OBJECT_ID('[kt].[SearchLog2Attendance]') IS NULL
    BEGIN
        CREATE TABLE [kt].[SearchLog2Attendance](
                                      [Id] [int] IDENTITY(1, 1) NOT NULL,
                                      [SearchLogId] [int] NOT NULL,
                                      [AttendanceId] int NOT NULL
        )
    END;

IF OBJECT_ID('kt.[PK_SearchLog]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[SearchLog] ADD CONSTRAINT [PK_SearchLog] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_SearchLog2LocationGroup]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[SearchLog2LocationGroup] ADD CONSTRAINT [PK_SearchLog2LocationGroup] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('kt.[PK_SearchLog2Attendance]', 'PK') IS NULL
    BEGIN
        ALTER TABLE [kt].[SearchLog2Attendance] ADD CONSTRAINT [PK_SearchLog2Attendance] PRIMARY KEY CLUSTERED ( [Id] ASC )
            WITH (DATA_COMPRESSION=ROW)
    END;

IF OBJECT_ID('[kt].[FK_SearchLog2LocationGroup_SearchLogId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[SearchLog2LocationGroup] ADD CONSTRAINT [FK_SearchLog2LocationGroup_SearchLogId]
            FOREIGN KEY ([SearchLogId])
                REFERENCES [kt].[SearchLog] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_SearchLog2LocationGroup_LocationGroupId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[SearchLog2LocationGroup] ADD CONSTRAINT [FK_SearchLog2LocationGroup_LocationGroupId]
            FOREIGN KEY ([LocationGroupId])
                REFERENCES [kt].[LocationGroup] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_SearchLog2Attendance_SearchLogId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[SearchLog2Attendance] ADD CONSTRAINT [FK_SearchLog2Attendance_SearchLogId]
            FOREIGN KEY ([SearchLogId])
                REFERENCES [kt].[SearchLog] ([Id])
    END;

IF OBJECT_ID('[kt].[FK_SearchLog2Attendance_AttendanceId]', 'F') IS NULL
    BEGIN
        ALTER TABLE [kt].[SearchLog2Attendance] ADD CONSTRAINT [FK_SearchLog2Attendance_AttendanceId]
            FOREIGN KEY ([AttendanceId])
                REFERENCES [kt].[Attendance] ([Id])
    END;
