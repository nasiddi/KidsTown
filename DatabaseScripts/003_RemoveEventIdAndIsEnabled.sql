USE CheckInsExtension;

IF COL_LENGTH('cie.Attendance','EventId') IS NOT NULL
BEGIN
ALTER TABLE [cie].[Attendance] DROP COLUMN [EventId]
END;

IF COL_LENGTH('cie.Location','IsEnabled') IS NOT NULL
    BEGIN
        ALTER TABLE [cie].[Location] DROP COLUMN [IsEnabled]
    END;