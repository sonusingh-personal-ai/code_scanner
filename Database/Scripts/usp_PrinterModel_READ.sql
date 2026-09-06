USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrinterModel_READ]    Script Date: 8/14/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_PrinterModel_READ]
	@Id INT = NULL
AS
BEGIN
	IF @Id IS NOT NULL
		SELECT * FROM PrinterModel WHERE Id = @Id
	ELSE
		SELECT * FROM PrinterModel ORDER BY Name
END
GO