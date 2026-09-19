USE [BarCodeSannerV3]
GO
/****** Object:  StoredProcedure [dbo].[usp_Model_DELETE]    Script Date: 8/9/2026 4:21:52 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[usp_Model_DELETE]
@ID INT
AS
BEGIN
	DELETE
		FROM
			Model
		WHERE 
			ID = @ID
END
GO