USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrinterModel_UPDATE]    Script Date: 8/14/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_PrinterModel_UPDATE]
	@Id INT,
	@Name NVARCHAR(255),
	@CreatedOn DATETIME,
	@ModifiedOn DATETIME
AS
BEGIN
	UPDATE PrinterModel 
	SET Name = @Name, ModifiedOn = @ModifiedOn
	WHERE Id = @Id
END
GO