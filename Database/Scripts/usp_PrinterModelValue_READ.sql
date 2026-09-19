USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrinterModelValue_READ]    Script Date: 8/15/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_PrinterModelValue_READ]
	@Id INT = NULL
AS
BEGIN
	IF @Id IS NOT NULL
		SELECT * FROM PrinterModelValue WHERE Id = @Id
	ELSE
		SELECT * FROM PrinterModelValue ORDER BY ModelId, Name
END
GO
