-- STREAMING_CHUNK:Configuring target database context...

--------------------------------------------------------------------------------
-- Database: SQL Server (T-SQL)
-- Target Database: BarCodeSannerV3
-- Target Table: dbo.Response
--------------------------------------------------------------------------------

USE [BarCodeSannerV3];
GO


-- Check if column exists in table 'Response' before adding it
IF NOT EXISTS (
    SELECT 1 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'dbo'
      AND TABLE_NAME = 'Response'         
      AND COLUMN_NAME = 'PrinterModelId'
)
BEGIN
    -- Add the new column to dbo.Response
    ALTER TABLE [dbo].[Response]
    ADD [PrinterModelId] INT NULL; -- Change INT to VARCHAR(50) if your printer IDs are string values

    PRINT 'Column [PrinterModelId] added successfully to [dbo].[Response].';
END
ELSE
BEGIN
    PRINT 'Column [PrinterModelId] already exists in [dbo].[Response]. Skipping execution.';
END
GO