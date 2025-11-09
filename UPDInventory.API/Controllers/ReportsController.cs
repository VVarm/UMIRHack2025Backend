using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPDInventory.Core.Data;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Enums;
using System.Security.Claims;

namespace UPDInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(AppDbContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("inventory/discrepancies")]
        public async Task<ActionResult> GetInventoryDiscrepancies([FromQuery] int organizationId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            // Проверяем доступ к организации
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null)
                return NotFound(new { message = "Организация не найдена" });

            if (!await _context.UserOrganizations.AnyAsync(uo => uo.UserId == userId.Value && uo.OrganizationId == organizationId))
                return Forbid();

            var query = _context.DocumentItems
                .Include(di => di.Document)
                .Include(di => di.Product)
                .Where(di => di.Document.OrganizationId == organizationId &&
                            di.Document.Type == DocumentType.Inventory &&
                            di.Document.Status == "completed");

            // Фильтрация по дате
            if (fromDate.HasValue)
            {
                query = query.Where(di => di.Document.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(di => di.Document.CreatedAt <= toDate.Value);
            }

            var discrepancies = await query
                .Where(di => di.QuantityExpected != di.QuantityActual)
                .Select(di => new
                {
                    DocumentId = di.DocumentId,
                    DocumentNumber = di.Document.Number,
                    DocumentDate = di.Document.DocumentDate,
                    ProductId = di.ProductId,
                    ProductName = di.Product.Name,
                    Barcode = di.Product.Barcode,
                    ExpectedQuantity = di.QuantityExpected,
                    ActualQuantity = di.QuantityActual,
                    Difference = di.QuantityActual - di.QuantityExpected,
                    AbsoluteDifference = Math.Abs(di.QuantityActual - di.QuantityExpected),
                    Unit = di.Product.Unit
                })
                .OrderByDescending(d => d.AbsoluteDifference)
                .ToListAsync();

            _logger.LogInformation("Сформирован отчет по расхождениям для организации {OrganizationId}", organizationId);

            return Ok(new
            {
                Organization = organization.Name,
                Period = new { From = fromDate, To = toDate },
                TotalDiscrepancies = discrepancies.Count,
                Discrepancies = discrepancies
            });
        }

        [HttpGet("inventory/statistics")]
        public async Task<ActionResult> GetInventoryStatistics([FromQuery] int organizationId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            // Проверяем доступ к организации
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null)
                return NotFound(new { message = "Организация не найдена" });

            if (!await _context.UserOrganizations.AnyAsync(uo => uo.UserId == userId.Value && uo.OrganizationId == organizationId))
                return Forbid();

            var query = _context.Documents
                .Where(d => d.OrganizationId == organizationId &&
                           d.Type == DocumentType.Inventory &&
                           d.Status == "completed");

            // Фильтрация по дате
            if (fromDate.HasValue)
            {
                query = query.Where(d => d.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.CreatedAt <= toDate.Value);
            }

            var documents = await query
                .Include(d => d.Items)
                .ToListAsync();

            var statistics = new
            {
                TotalInventories = documents.Count,
                TotalItemsScanned = documents.Sum(d => d.Items.Count),
                TotalItemsWithDiscrepancies = documents.Sum(d => d.Items.Count(i => i.QuantityExpected != i.QuantityActual)),
                AverageDiscrepancyRate = documents.Any() ? 
                    (double)documents.Sum(d => d.Items.Count(i => i.QuantityExpected != i.QuantityActual)) / documents.Sum(d => d.Items.Count) * 100 : 0,
                InventoriesByMonth = documents
                    .GroupBy(d => new { d.CreatedAt.Year, d.CreatedAt.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:00}",
                        Count = g.Count(),
                        ItemsScanned = g.Sum(d => d.Items.Count),
                        ItemsWithDiscrepancies = g.Sum(d => d.Items.Count(i => i.QuantityExpected != i.QuantityActual))
                    })
                    .OrderBy(g => g.Period)
                    .ToList()
            };

            _logger.LogInformation("Сформирована статистика по инвентаризациям для организации {OrganizationId}", organizationId);

            return Ok(new
            {
                Organization = organization.Name,
                Period = new { From = fromDate, To = toDate },
                Statistics = statistics
            });
        }

        [HttpGet("products/most-scanned")]
        public async Task<ActionResult> GetMostScannedProducts([FromQuery] int organizationId, [FromQuery] int top = 10)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            // Проверяем доступ к организации
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null)
                return NotFound(new { message = "Организация не найдена" });

            if (!await _context.UserOrganizations.AnyAsync(uo => uo.UserId == userId.Value && uo.OrganizationId == organizationId))
                return Forbid();

            var mostScanned = await _context.DocumentItems
                .Include(di => di.Product)
                .Include(di => di.Document)
                .Where(di => di.Document.OrganizationId == organizationId &&
                            di.Document.Type == DocumentType.Inventory &&
                            di.Document.Status == "completed")
                .GroupBy(di => new { di.ProductId, di.Product.Name, di.Product.Barcode, di.Product.Unit })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    Barcode = g.Key.Barcode,
                    Unit = g.Key.Unit,
                    ScanCount = g.Count(),
                    TotalExpected = g.Sum(x => x.QuantityExpected),
                    TotalActual = g.Sum(x => x.QuantityActual),
                    AverageDiscrepancy = g.Average(x => Math.Abs(x.QuantityActual - x.QuantityExpected))
                })
                .OrderByDescending(x => x.ScanCount)
                .Take(top)
                .ToListAsync();

            _logger.LogInformation("Сформирован отчет по самым сканируемым товарам для организации {OrganizationId}", organizationId);

            return Ok(new
            {
                Organization = organization.Name,
                MostScannedProducts = mostScanned
            });
        }

        [HttpGet("documents/summary")]
        public async Task<ActionResult> GetDocumentsSummary([FromQuery] int organizationId, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            // Проверяем доступ к организации
            var organization = await _context.Organizations.FindAsync(organizationId);
            if (organization == null)
                return NotFound(new { message = "Организация не найдена" });

            if (!await _context.UserOrganizations.AnyAsync(uo => uo.UserId == userId.Value && uo.OrganizationId == organizationId))
                return Forbid();

            var query = _context.Documents
                .Where(d => d.OrganizationId == organizationId);

            // Фильтрация по дате
            if (fromDate.HasValue)
            {
                query = query.Where(d => d.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(d => d.CreatedAt <= toDate.Value);
            }

            var summary = await query
                .GroupBy(d => new { d.Type, d.Status })
                .Select(g => new
                {
                    DocumentType = g.Key.Type.ToString(),
                    Status = g.Key.Status,
                    Count = g.Count(),
                    LastDocumentDate = g.Max(d => d.CreatedAt)
                })
                .ToListAsync();

            _logger.LogInformation("Сформирована сводка по документам для организации {OrganizationId}", organizationId);

            return Ok(new
            {
                Organization = organization.Name,
                Period = new { From = fromDate, To = toDate },
                Summary = summary
            });
        }

        private int? GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}