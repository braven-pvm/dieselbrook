using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AnqIntegrationApi.DbContexts;
using AnqIntegrationApi.Models.Nop;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AnqIntegrationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UploadController : ControllerBase
    {
        private readonly NopDbContext _nopDb;
        private readonly ILogger<UploadController> _logger;

        public UploadController(NopDbContext nopDb, ILogger<UploadController> logger)
        {
            _nopDb = nopDb;
            _logger = logger;
        }

        // ---------- DTOs ----------

        public record UploadError(int? Row, string? Column, string Code, string Message);

        public record UploadResponse(
            bool Success,
            int? DiscountId,
            int RowsInserted,
            int RowsSkippedOverlap,
            List<UploadError> Errors);

        public record DiscountLookupDto(
            int Id,
            string Name,
            string? CouponCode,
            bool IsActive);

        public sealed class UploadDiscountCustomersRequest
        {
            public IFormFile File { get; set; } = default!;
            public int DiscountId { get; set; }
        }

        private sealed record CustomerLookup(int Id, string Username);

        private sealed record ExistingRange(int CustomerId, DateTime? StartDateUtc, DateTime? EndDateUtc);

        // ---------- LOOKUP: Discounts ----------

        [HttpGet("discounts")]
        public async Task<IActionResult> GetDiscounts()
        {
            // Auth + DB resolution already handled by middleware
            var discounts = await _nopDb.Database.SqlQueryRaw<DiscountLookupDto>(
                @"SELECT Id, Name, CouponCode, IsActive
                  FROM dbo.Discount
                  ORDER BY IsActive DESC, Name")
                .ToListAsync();

            return Ok(discounts);
        }

        // ---------- UPLOAD ----------

        [HttpPost("excel-discount-customers")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UploadDiscountCustomers(
            [FromForm] UploadDiscountCustomersRequest request)
        {
            var errors = new List<UploadError>();
            var discountId = request.DiscountId;
            var file = request.File;

            if (discountId <= 0)
                return Ok(new UploadResponse(false, null, 0, 0,
                    new() { new UploadError(null, "DiscountId", "REQUIRED", "DiscountId is required.") }));

            if (file == null || file.Length == 0)
                return Ok(new UploadResponse(false, discountId, 0, 0,
                    new() { new UploadError(null, null, "FILE_EMPTY", "No file uploaded.") }));

            if (!Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                return Ok(new UploadResponse(false, discountId, 0, 0,
                    new() { new UploadError(null, null, "FILE_TYPE", "Only .xlsx files are supported.") }));

            // Confirm discount exists
            var discountExists = await _nopDb.Database
       .SqlQueryRaw<int>(
           "SELECT COUNT(1) AS Value FROM dbo.Discount WHERE Id = {0}",
           discountId)
       .AnyAsync();


            if (!discountExists)
                return Ok(new UploadResponse(false, discountId, 0, 0,
                    new() { new UploadError(null, "DiscountId", "NOT_FOUND", "Discount not found.") }));

            // ---------- Parse Excel ----------
            List<ParsedRow> parsedRows;
            try
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                parsedRows = ParseExcel(ms, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excel parsing failed");
                return Ok(new UploadResponse(false, discountId, 0, 0,
                    new() { new UploadError(null, null, "PARSE_FAILED", "Unable to read Excel file.") }));
            }

            if (errors.Any())
                return Ok(new UploadResponse(false, discountId, 0, 0, errors));

            // ---------- De-dupe duplicate usernames in Excel (keep first occurrence) ----------
            parsedRows = parsedRows
                .GroupBy(r => r.UserName?.Trim() ?? "", StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderBy(x => x.RowNumber).First())
                .ToList();


            // ---------- Resolve Customers by Username ----------
            var usernames = parsedRows
                .Select(r => r.UserName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var usernamesJson = JsonSerializer.Serialize(usernames);

            var customers = await _nopDb.Database.SqlQueryRaw<CustomerLookup>(
                @"SELECT Id, Username
                  FROM dbo.Customer
                  WHERE Username IN (SELECT [value] FROM OPENJSON({0}))",
                usernamesJson).ToListAsync();

            var customerMap = customers
                .ToDictionary(c => c.Username, c => c.Id, StringComparer.OrdinalIgnoreCase);



            // Resolve CustomerId by Username PER ROW (no map)
            var inserts = new List<AnqDiscountAppliedToCustomer>();

            foreach (var row in parsedRows)
            {
                // Raw SQL: get CustomerId for this username (Username only, no email)
                var customerId = await _nopDb.Database.SqlQueryRaw<int>(
                    @"SELECT TOP (1) Id AS Value
          FROM dbo.Customer
          WHERE Username = {0}",
                    row.UserName).FirstOrDefaultAsync();

                if (customerId <= 0)
                {
                    errors.Add(new UploadError(row.RowNumber, "UserName", "NOT_FOUND",
                        $"Customer '{row.UserName}' not found."));
                    _logger.LogWarning("Not Found({row.Username})", row.UserName);
                    continue;
                }

                // Duplicate/overlap check per row (DiscountId + CustomerId + overlap)
                // Note: alias required for SqlQueryRaw<int>
                var overlapExists = await _nopDb.Database.SqlQueryRaw<int>(
                    @"SELECT TOP (1) CAST(1 AS int) AS Value
          FROM dbo.ANQ_Discount_AppliedToCustomers e
          WHERE e.DiscountId = {0}
            AND e.CustomerId = {1}
            AND e.IsActive = 1
            AND (e.EndDateUtc IS NULL OR e.EndDateUtc >= {2})
            AND ({3} IS NULL OR {3} >= e.StartDateUtc)",
                    discountId,
                    customerId,
                    row.FromDateUtc,
                    row.ToDateUtc
                ).AnyAsync();

                if (overlapExists)
                {
                    // skip (not an error)
                    continue;
                }
                _logger.LogInformation("Added({row.Username})", row.UserName);

               inserts.Add(new AnqDiscountAppliedToCustomer
                {
                    DiscountId = discountId,
                    CustomerId = customerId,
                    IsActive = true,
                    StartDateUtc = row.FromDateUtc,
                    EndDateUtc = row.ToDateUtc,
                    DiscountLimitationId = 25,
                    LimitationTimes = 1,
                    NoTimesUsed = 0,
                    Notified = false,
                    Comment = row.Comment,
                   NotifyWhatsApp = row.Whatsapp?
        .Trim()
        .StartsWith("Y", StringComparison.OrdinalIgnoreCase) == true
               });
            }

            if (errors.Any())
                return Ok(new UploadResponse(false, discountId, 0, 0, errors));

            // ---------- Duplicate / Overlap Handling ----------
            var customerIds = inserts.Select(i => i.CustomerId).Distinct().ToList();
            var customerIdsJson = JsonSerializer.Serialize(customerIds);

            var existingRanges = await _nopDb.Database.SqlQueryRaw<ExistingRange>(
                @"SELECT CustomerId, StartDateUtc, EndDateUtc
                  FROM dbo.ANQ_Discount_AppliedToCustomers
                  WHERE DiscountId = {0}
                    AND IsActive = 1
                    AND CustomerId IN (SELECT CAST([value] AS int) FROM OPENJSON({1}))",
                discountId, customerIdsJson).ToListAsync();

            var existingByCustomer = existingRanges
                .GroupBy(e => e.CustomerId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var finalInserts = new List<AnqDiscountAppliedToCustomer>();
            var skipped = 0;

            foreach (var ins in inserts)
            {
                if (existingByCustomer.TryGetValue(ins.CustomerId, out var ranges) &&
                    ranges.Any(r => RangesOverlap(ins.StartDateUtc, ins.EndDateUtc, r.StartDateUtc, r.EndDateUtc)))
                {
                    skipped++;
                    continue;
                }

                finalInserts.Add(ins);
            }

            if (finalInserts.Any())
            {
                // IMPORTANT: DbSet name is singular in your context
                _nopDb.AnqDiscountAppliedToCustomer.AddRange(finalInserts);
                await _nopDb.SaveChangesAsync();
            }

            return Ok(new UploadResponse(true, discountId, finalInserts.Count, skipped, new()));
        }

        // ---------- Helpers ----------

        private sealed class ParsedRow
        {
            public int RowNumber { get; init; }
            public string UserName { get; init; } = "";
            public DateTime FromDateUtc { get; init; }
            public DateTime ToDateUtc { get; init; }
            public string? Comment { get; init; }
            public string? Whatsapp { get; init; }
        }

        private static List<ParsedRow> ParseExcel(Stream stream, List<UploadError> errors)
        {
            using var wb = new XLWorkbook(stream);
            var ws = wb.Worksheets.First();

            var today = DateTime.UtcNow.Date;
            var rows = new List<ParsedRow>();

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            for (int r = 2; r <= lastRow; r++)
            {
                var user = ws.Cell(r, 1).GetString().Trim();
                if (string.IsNullOrWhiteSpace(user))
                {
                    errors.Add(new UploadError(r, "UserName", "REQUIRED", "UserName is required."));
                    continue;
                }

                if (!ws.Cell(r, 2).TryGetValue<DateTime>(out var from) ||
                    !ws.Cell(r, 3).TryGetValue<DateTime>(out var to))
                {
                    errors.Add(new UploadError(r, null, "DATE", "FromDate and ToDate are required."));
                    continue;
                }

                from = from.Date;
                to = to.Date;

                if (from < today || to < today)
                    errors.Add(new UploadError(r, null, "DATE_PAST", "Dates must be today or later."));

                if (to < from)
                    errors.Add(new UploadError(r, "ToDate", "RANGE", "ToDate must be >= FromDate."));

                if (errors.Any())
                    continue;

                rows.Add(new ParsedRow
                {
                    RowNumber = r,
                    UserName = user,
                    FromDateUtc = DateTime.SpecifyKind(from, DateTimeKind.Utc),
                    ToDateUtc = DateTime.SpecifyKind(to, DateTimeKind.Utc),
                    Comment = ws.Cell(r, 4).GetString().Trim(),
                    Whatsapp = ws.Cell(r, 5).GetString().Trim()
                });
            }

            if (!rows.Any())
                errors.Add(new UploadError(null, null, "NO_DATA", "No valid rows found."));

            return rows;
        }

        private static bool RangesOverlap(
            DateTime? nStart, DateTime? nEnd,
            DateTime? eStart, DateTime? eEnd)
        {
            return (!eEnd.HasValue || !nStart.HasValue || eEnd >= nStart)
                && (!nEnd.HasValue || !eStart.HasValue || nEnd >= eStart);
        }
    }
}
