USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_PrinterModelValue_DELETE]    Script Date: 8/15/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_PrinterModelValue_DELETE]
	@Id INT
AS
BEGIN
	DELETE
		FROM
			PrinterModelValue
		WHERE 
			Id = @Id
END
GO
