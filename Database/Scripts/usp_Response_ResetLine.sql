USE [BarCodeSannerV3];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

-- Stored procedure to set Response.Line to NULL when a line or all lines are deleted
CREATE OR ALTER PROCEDURE [dbo].[usp_Response_ResetLine]
    @LineId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (@LineId IS NOT NULL AND @LineId > 0)
    BEGIN
        UPDATE [dbo].[Response]
        SET [Line] = NULL
        WHERE [Line] = @LineId;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[Response]
        SET [Line] = NULL;
    END
END
GO
