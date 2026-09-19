USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrinterModelValue_UPDATE]    Script Date: 8/15/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_PrinterModelValue_UPDATE]
	@Id INT,
	@ModelId INT,
	@Name NVARCHAR(255),
	@Value NVARCHAR(MAX),
	@CreatedOn DATETIME,
	@ModifiedOn DATETIME
AS
BEGIN
	UPDATE PrinterModelValue 
	SET ModelId = @ModelId, Name = @Name, Value = @Value, ModifiedOn = @ModifiedOn
	WHERE Id = @Id
END
GO
