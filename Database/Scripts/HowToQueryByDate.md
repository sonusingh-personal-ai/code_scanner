# How to Query Response Data by Date

## Stored Procedure: usp_Response_READ

The stored procedure has multiple conditional branches. Here's how to query for a specific date:

## Method 1: Using CurrentDate (RECOMMENDED)

**Use this method to get all records for a specific date without pagination limits.**

### SQL Query:
```sql
EXEC usp_Response_READ 
    @StartRowNumber = NULL,
    @EndRowNumber = NULL,
    @StartDate = NULL,
    @EndDate = NULL,
    @CurrentDate = '09/08/2026',  -- Date in dd/MM/yyyy format
    @Id = NULL,
    @Barcode = NULL,
    @QcStatus = NULL,
    @SearchStr = NULL
```

### C# Code:
```csharp
var todayString = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
objENResponse.CurrentDate = todayString;
listOfResponses = objBLResponse.ReadAllAndAggregate(null, null, null, null, todayString, typeof(enResponseSummary));
```

**Result:** Returns ALL records where `CurrentDate = '09/08/2026'` (no limit)

---

## Method 2: Using CreatedOn Date Range

**Use this method to get records by datetime range with pagination.**

### SQL Query:
```sql
EXEC usp_Response_READ 
    @StartRowNumber = 1,                    -- Required for pagination
    @EndRowNumber = 1000,                   -- Page size
    @StartDate = '2026-08-09 00:00:00.000', -- Start of day
    @EndDate = '2026-08-09 23:59:59.999',   -- End of day
    @CurrentDate = NULL,
    @Id = NULL,
    @Barcode = NULL,
    @QcStatus = NULL,
    @SearchStr = NULL
```

### C# Code:
```csharp
var startDate = DateTime.Today;                    // 2026-08-09 00:00:00
var endDate = DateTime.Today.AddDays(1);            // 2026-08-10 00:00:00
listOfResponses = objBLResponse.ReadAllAndAggregate(1, 1000, startDate, endDate, null, typeof(enResponseSummary));
```

**Result:** Returns up to 1000 records where `CreatedOn` is between start and end datetime

---

## Stored Procedure Logic Flow:

The procedure checks conditions in this order:

1. `IF(@ID > 0)` - Filter by ID only
2. `ELSE IF(@Barcode != '' AND @QcStatus > 0)` - Filter by Barcode + QCStatus
3. `ELSE IF(@Barcode != '')` - Filter by Barcode only
4. `ELSE IF(@SearchStr != '' AND @StartDate IS NOT NULL AND @EndDate IS NOT NULL and @StartRowNumber IS NOT NULL)` - Date range + Search + Pagination
5. `ELSE IF(@SearchStr != '' and @StartRowNumber IS NOT NULL)` - Search + Pagination
6. `ELSE IF(@StartDate IS NOT NULL AND @EndDate IS NOT NULL and @StartRowNumber IS NOT NULL)` - **Date range + Pagination**
7. `ELSE IF(@StartRowNumber IS NOT NULL)` - Pagination only
8. `ELSE IF(@CurrentDate != '')` - **Filter by CurrentDate (NO pagination)** ← USE THIS FOR TODAY'S DATA
9. `ELSE` - Get all records

---

## Important Notes:

### ⚠️ Critical: CurrentDate Format
- **Format:** `dd/MM/yyyy` (e.g., "09/08/2026" for August 9, 2026)
- **Type:** String (NVARCHAR(50))
- **Storage:** Stored as string in database, NOT as DateTime

### ⚠️ Critical: CreatedOn Format
- **Format:** `yyyy-MM-dd HH:mm:ss.fff` (e.g., "2026-08-09 00:00:00.000")
- **Type:** DateTime
- **Storage:** Stored as DateTime in database

### ⚠️ Pagination Requirement
- When using `@StartDate` and `@EndDate`, you MUST also provide `@StartRowNumber` and `@EndRowNumber`
- If you don't provide row numbers, the date range filter is SKIPPED
- When using `@CurrentDate`, pagination is NOT required

---

## For ExportService:

The ExportService now uses **Method 1 (CurrentDate)** which:
- ✅ Returns ALL records for today (no 10-record limit)
- ✅ No pagination required
- ✅ Matches how dates are stored in the database
- ✅ Simple and reliable

If you need to use date range instead, update the stored procedure call to include row numbers.