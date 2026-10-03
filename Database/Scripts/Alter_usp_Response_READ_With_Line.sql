USE [BarCodeSannerV3];
GO

CREATE OR ALTER PROCEDURE [dbo].[usp_Response_READ]
    @StartRowNumber INT = NULL,
    @EndRowNumber INT = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @CurrentDate NVARCHAR(50) = NULL,
    @Id INT = NULL,
    @Barcode NVARCHAR(50) = NULL,
    @QcStatus INT = NULL,
    @SearchStr NVARCHAR(100) = NULL,
    @Line INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF (ISNULL(@Line, 0) <= 0)
        SET @Line = NULL;

    -- 1. Search by ID
    IF (@ID IS NOT NULL AND @ID > 0)
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            1 AS RowNumber,
            1 AS TotalRecords
        FROM Response
        WHERE ID = @ID;
    END

    -- 2. Search by Barcode and QcStatus
    ELSE IF (ISNULL(@Barcode, '') != '' AND ISNULL(@QcStatus, 0) > 0)
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            RowNumber = ROW_NUMBER() OVER (ORDER BY ID DESC),
            TotalRecords = COUNT(*) OVER()
        FROM Response
        WHERE Barcode = @Barcode AND QcStatus = @QcStatus
          AND (@Line IS NULL OR Line = @Line);
    END

    -- 3. Search by Barcode only
    ELSE IF (ISNULL(@Barcode, '') != '')
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            RowNumber = ROW_NUMBER() OVER (ORDER BY ID DESC),
            TotalRecords = COUNT(*) OVER()
        FROM Response
        WHERE Barcode = @Barcode
          AND (@Line IS NULL OR Line = @Line);
    END

    -- 4. Search by SearchStr + Date Range + Pagination
    ELSE IF (ISNULL(@SearchStr, '') != '' AND @StartDate IS NOT NULL AND @EndDate IS NOT NULL AND @StartRowNumber IS NOT NULL)
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            RowNumber = ROW_NUMBER() OVER (ORDER BY ID DESC),
            TotalRecords = COUNT(*) OVER()
        FROM Response
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate
          AND (@Line IS NULL OR Line = @Line)
          AND (Barcode LIKE '%' + @SearchStr + '%'	
               OR Model LIKE '%' + @SearchStr + '%'	
               OR SystemRating LIKE '%' + @SearchStr + '%'	
               OR SerialCardNo LIKE '%' + @SearchStr + '%')
        ORDER BY ID DESC
        OFFSET (@StartRowNumber - 1) ROWS
        FETCH NEXT (@EndRowNumber - @StartRowNumber + 1) ROWS ONLY;
    END

    -- 5. Search by SearchStr + Pagination
    ELSE IF (ISNULL(@SearchStr, '') != '' AND @StartRowNumber IS NOT NULL)
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            RowNumber = ROW_NUMBER() OVER (ORDER BY ID DESC),
            TotalRecords = COUNT(*) OVER()
        FROM Response
        WHERE (@Line IS NULL OR Line = @Line)
          AND (Barcode LIKE '%' + @SearchStr + '%'	
               OR Model LIKE '%' + @SearchStr + '%'	
               OR SystemRating LIKE '%' + @SearchStr + '%'	
               OR SerialCardNo LIKE '%' + @SearchStr + '%')
        ORDER BY ID DESC
        OFFSET (@StartRowNumber - 1) ROWS
        FETCH NEXT (@EndRowNumber - @StartRowNumber + 1) ROWS ONLY;
    END

    -- 6. Filter ONLY by CreatedOn actual date timestamp
    ELSE IF (ISNULL(@CurrentDate, '') != '')
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            RowNumber = ROW_NUMBER() OVER (ORDER BY ID DESC),
            TotalRecords = COUNT(*) OVER()
        FROM Response
        WHERE CAST(CreatedOn AS DATE) = TRY_CONVERT(DATE, @CurrentDate, 103)
          AND (@Line IS NULL OR Line = @Line)
        ORDER BY ID DESC;
    END

    -- 7. Search by Date Range + Pagination
    ELSE IF (@StartDate IS NOT NULL AND @EndDate IS NOT NULL AND @StartRowNumber IS NOT NULL)
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            RowNumber = ROW_NUMBER() OVER (ORDER BY ID DESC),
            TotalRecords = COUNT(*) OVER()
        FROM Response
        WHERE CreatedOn >= @StartDate AND CreatedOn <= @EndDate
          AND (@Line IS NULL OR Line = @Line)
        ORDER BY ID DESC
        OFFSET (@StartRowNumber - 1) ROWS
        FETCH NEXT (@EndRowNumber - @StartRowNumber + 1) ROWS ONLY;
    END

    -- 8. Fallback: Return all records with pagination if StartRowNumber is provided
    ELSE IF (@StartRowNumber IS NOT NULL)
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            RowNumber = ROW_NUMBER() OVER (ORDER BY ID DESC),
            TotalRecords = COUNT(*) OVER()
        FROM Response
        WHERE (@Line IS NULL OR Line = @Line)
        ORDER BY ID DESC
        OFFSET (@StartRowNumber - 1) ROWS
        FETCH NEXT (@EndRowNumber - @StartRowNumber + 1) ROWS ONLY;
    END

    -- 9. Fallback: Return everything
    ELSE
    BEGIN
        SELECT	
            Id, Barcode, QcStatus, VisualBy, TestedBy, ProductionLine,
            ProcessEngg, SerialCardNo, Model, ConProgNo, DisProgNo,
            SystemRating, CurrentDate, CurrentTime, ResponseTime,
            CreatedOn, PrinterModelId, Line,
            RowNumber = ROW_NUMBER() OVER (ORDER BY ID DESC),
            TotalRecords = COUNT(*) OVER()
        FROM Response
        WHERE (@Line IS NULL OR Line = @Line);
    END
END
GO
