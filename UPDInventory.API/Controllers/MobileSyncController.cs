using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPDInventory.Core.DTOs;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;

namespace UPDInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MobileSyncController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMobileSessionService _mobileSessionService;

        public MobileSyncController(AppDbContext context, IMobileSessionService mobileSessionService)
        {
            _context = context;
            _mobileSessionService = mobileSessionService;
        }

        [HttpPost("init")]
        public async Task<ActionResult<MobileSyncResponse>> InitializeSync([FromBody] string sessionToken)
        {
            try
            {
                // Валидируем и используем сессию
                var session = await _mobileSessionService.ValidateAndUseSessionAsync(sessionToken);
                
                var organizationId = session.OrganizationId;

                // Получаем все данные для организации
                var organization = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.Id == organizationId);

                if (organization == null)
                {
                    return NotFound(new { message = "Организация не найдена" });
                }

                var warehouses = await _context.Warehouses
                    .Where(w => w.OrganizationId == organizationId && w.IsActive)
                    .ToListAsync();

                var products = await _context.Products
                    .Where(p => p.OrganizationId == organizationId && p.IsActive)
                    .ToListAsync();

                var documents = await _context.Documents
                    .Include(d => d.Items)
                    .ThenInclude(i => i.Product)
                    .Where(d => d.OrganizationId == organizationId)
                    .OrderByDescending(d => d.CreatedAt)
                    .Take(100) // Ограничиваем количество документов для первой синхронизации
                    .ToListAsync();

                var response = new MobileSyncResponse
                {
                    Organization = organization,
                    Warehouses = warehouses,
                    Products = products,
                    Documents = documents,
                    SessionToken = sessionToken,
                    SyncTimestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("data")]
        public async Task<ActionResult<MobileSyncResponse>> SyncData([FromBody] MobileSyncRequest request)
        {
            try
            {
                // Проверяем валидность сессии
                if (!await _mobileSessionService.IsSessionValidAsync(request.SessionToken))
                {
                    return Unauthorized(new { message = "Недействительная сессия" });
                }

                var session = await _context.MobileSessions
                    .FirstOrDefaultAsync(ms => ms.Token == request.SessionToken);

                if (session == null)
                {
                    return Unauthorized(new { message = "Сессия не найдена" });
                }

                var organizationId = session.OrganizationId;

                // Получаем обновленные данные с момента последней синхронизации
                var organization = await _context.Organizations
                    .FirstOrDefaultAsync(o => o.Id == organizationId);

                if (organization == null)
                {
                    return NotFound(new { message = "Организация не найдена" });
                }

                var warehouses = await _context.Warehouses
                    .Where(w => w.OrganizationId == organizationId && w.IsActive)
                    .ToListAsync();

                var products = await _context.Products
                    .Where(p => p.OrganizationId == organizationId && p.IsActive)
                    .ToListAsync();

                var documents = await _context.Documents
                    .Include(d => d.Items)
                    .ThenInclude(i => i.Product)
                    .Where(d => d.OrganizationId == organizationId && d.CreatedAt > request.LastSyncTimestamp)
                    .OrderByDescending(d => d.CreatedAt)
                    .ToListAsync();

                var response = new MobileSyncResponse
                {
                    Organization = organization,
                    Warehouses = warehouses,
                    Products = products,
                    Documents = documents,
                    SessionToken = request.SessionToken,
                    SyncTimestamp = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при синхронизации данных", error = ex.Message });
            }
        }

        [HttpPost("documents")]
        public async Task<ActionResult> UploadDocuments([FromBody] MobileDocumentUploadRequest request)
        {
            try
            {
                // Проверяем валидность сессии
                if (!await _mobileSessionService.IsSessionValidAsync(request.SessionToken))
                {
                    return Unauthorized(new { message = "Недействительная сессия" });
                }

                var session = await _context.MobileSessions
                    .FirstOrDefaultAsync(ms => ms.Token == request.SessionToken);

                if (session == null)
                {
                    return Unauthorized(new { message = "Сессия не найдена" });
                }

                // Сохраняем документы из мобильного приложения
                foreach (var document in request.Documents)
                {
                    // Убеждаемся, что документ принадлежит правильной организации
                    document.OrganizationId = session.OrganizationId;
                    
                    if (document.Id == 0) // Новый документ
                    {
                        _context.Documents.Add(document);
                    }
                    else // Обновление существующего
                    {
                        _context.Documents.Update(document);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Документы успешно загружены", count = request.Documents.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при загрузке документов", error = ex.Message });
            }
        }
    }

    // DTO для запроса синхронизации
    public class MobileSyncRequest
    {
        public string SessionToken { get; set; } = string.Empty;
        public DateTime LastSyncTimestamp { get; set; }
    }

    // DTO для загрузки документов
    public class MobileDocumentUploadRequest
    {
        public string SessionToken { get; set; } = string.Empty;
        public List<Document> Documents { get; set; } = new List<Document>();
    }
}