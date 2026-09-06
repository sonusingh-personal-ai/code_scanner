USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrinterModel_CREATE]    Script Date: 8/14/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_PrinterModel_CREATE]
	@Name NVARCHAR(255),
	@CreatedOn DATETIME
AS
BEGIN
	INSERT INTO PrinterModel (Name, CreatedOn)
	VALUES (@Name, @CreatedOn)
	
	SELECT SCOPE_IDENTITY() AS Id
END
GO