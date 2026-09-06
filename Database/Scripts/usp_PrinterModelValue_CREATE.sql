USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrinterModelValue_CREATE]    Script Date: 8/15/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_PrinterModelValue_CREATE]
	@ModelId INT,
	@Name NVARCHAR(255),
	@Value NVARCHAR(MAX),
	@CreatedOn DATETIME
AS
BEGIN
	INSERT INTO PrinterModelValue (ModelId, Name, Value, CreatedOn)
	VALUES (@ModelId, @Name, @Value, @CreatedOn)
	
	SELECT SCOPE_IDENTITY() AS Id
END
GO
