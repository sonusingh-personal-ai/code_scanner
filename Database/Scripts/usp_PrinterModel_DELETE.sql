USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrinterModel_DELETE]    Script Date: 8/14/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_PrinterModel_DELETE]
	@Id INT
AS
BEGIN
	DELETE
		FROM
			PrinterModel
		WHERE 
			Id = @Id
END
GO