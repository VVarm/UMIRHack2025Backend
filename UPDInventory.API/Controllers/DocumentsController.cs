using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Enums;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;
using UPDInventory.Core.DTOs;
using System.Security.Claims;

namespace UPDInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOrganizationService _organizationService;

        public DocumentsController(AppDbContext context, IOrganizationService organizationService)
        {
            _context = context;
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Document>>> GetDocuments([FromQuery] int? organizationId, [FromQuery] DocumentType? type)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            IQueryable<Document> query = _context.Documents
                .Include(d => d.Organization)
                .Include(d => d.SourceWarehouse)
                .Include(d => d.DestinationWarehouse)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Product);

            // Фильтрация по организации
            if (organizationId.HasValue)
            {
                if (!await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, organizationId.Value))
                    return Forbid();

                query = query.Where(d => d.OrganizationId == organizationId.Value);
            }
            else
            {
                var userOrganizations = await _organizationService.GetUserOrganizationsAsync(userId.Value);
                var organizationIds = userOrganizations.Select(o => o.Id);
                query = query.Where(d => organizationIds.Contains(d.OrganizationId));
            }

            // Фильтрация по типу документа
            if (type.HasValue)
            {
                query = query.Where(d => d.Type == type.Value);
            }

            var documents = await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(documents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Document>> GetDocument(int id)
        {
            var document = await _context.Documents
                .Include(d => d.Organization)
                .Include(d => d.SourceWarehouse)
                .Include(d => d.DestinationWarehouse)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (document == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, document.OrganizationId))
                return Forbid();

            return Ok(document);
        }

        [HttpPost]
        public async Task<ActionResult<Document>> CreateDocument(DocumentCreateDto documentDto)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, documentDto.OrganizationId))
                return Forbid();

            // Проверяем доступ к складам (если указаны)
            if (documentDto.SourceWarehouseId.HasValue)
            {
                var sourceWarehouse = await _context.Warehouses.FindAsync(documentDto.SourceWarehouseId.Value);
                if (sourceWarehouse == null || sourceWarehouse.OrganizationId != documentDto.OrganizationId)
                    return BadRequest(new { message = "Исходный склад не найден или принадлежит другой организации" });
            }

            if (documentDto.DestinationWarehouseId.HasValue)
            {
                var destinationWarehouse = await _context.Warehouses.FindAsync(documentDto.DestinationWarehouseId.Value);
                if (destinationWarehouse == null || destinationWarehouse.OrganizationId != documentDto.OrganizationId)
                    return BadRequest(new { message = "Целевой склад не найден или принадлежит другой организации" });
            }

            // Создаем Document из DTO
            var document = new Document
            {
                Type = documentDto.Type,
                Number = string.IsNullOrEmpty(documentDto.Number) ? await GenerateDocumentNumberAsync(documentDto.OrganizationId, documentDto.Type) : documentDto.Number,
                Status = "draft",
                Comment = documentDto.Comment,
                DocumentDate = documentDto.DocumentDate,
                OrganizationId = documentDto.OrganizationId,
                SourceWarehouseId = documentDto.SourceWarehouseId,
                DestinationWarehouseId = documentDto.DestinationWarehouseId,
                WarehouseId = documentDto.WarehouseId,
                CreatedByUserId = userId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            // Загружаем связанные данные для ответа
            await _context.Entry(document)
                .Reference(d => d.Organization)
                .LoadAsync();
            await _context.Entry(document)
                .Reference(d => d.SourceWarehouse)
                .LoadAsync();
            await _context.Entry(document)
                .Reference(d => d.DestinationWarehouse)
                .LoadAsync();
            await _context.Entry(document)
                .Reference(d => d.Warehouse)
                .LoadAsync();
            await _context.Entry(document)
                .Reference(d => d.CreatedBy)
                .LoadAsync();

            return CreatedAtAction(nameof(GetDocument), new { id = document.Id }, document);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, document.OrganizationId))
                return Forbid();

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{documentId}/add-product")]
        public async Task<ActionResult<ApiResponse<DocumentItem>>> AddProductToDocument(int documentId, AddProductRequest request)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
                return NotFound(ApiResponse<DocumentItem>.ErrorResponse("Документ не найден"));

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, document.OrganizationId))
                return Forbid();

            // Ищем товар по штрих-коду в организации документа
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.OrganizationId == document.OrganizationId && p.Barcode == request.ProductBarcode);

            if (product == null)
            {
                return NotFound(ApiResponse<DocumentItem>.ErrorResponse("Товар с указанным штрих-кодом не найден в организации"));
            }

            // Проверяем, есть ли уже такая позиция в документе
            var existingItem = await _context.DocumentItems
                .FirstOrDefaultAsync(i => i.DocumentId == documentId && i.ProductId == product.Id);

            if (existingItem != null)
            {
                // Обновляем количество существующей позиции
                existingItem.QuantityActual += (decimal)request.Quantity;
                await _context.SaveChangesAsync();

                // Загружаем связанные данные
                await _context.Entry(existingItem)
                    .Reference(i => i.Product)
                    .LoadAsync();

                return Ok(ApiResponse<DocumentItem>.SuccessResponse(existingItem, "Количество товара обновлено"));
            }
            else
            {
                // Создаем новую позицию
                var newItem = new DocumentItem
                {
                    DocumentId = documentId,
                    ProductId = product.Id,
                    QuantityExpected = 0, // Для инвентаризации ожидаемое количество может быть 0
                    QuantityActual = (decimal)request.Quantity
                };

                _context.DocumentItems.Add(newItem);
                await _context.SaveChangesAsync();

                // Загружаем связанные данные
                await _context.Entry(newItem)
                    .Reference(i => i.Product)
                    .LoadAsync();

                return Ok(ApiResponse<DocumentItem>.SuccessResponse(newItem, "Товар добавлен в документ"));
            }
        }

        [HttpPut("{documentId}/status")]
        public async Task<ActionResult<ApiResponse<Document>>> UpdateDocumentStatus(int documentId, [FromBody] string status)
        {
            var document = await _context.Documents
                .Include(d => d.Organization)
                .Include(d => d.SourceWarehouse)
                .Include(d => d.DestinationWarehouse)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
                return NotFound(ApiResponse<Document>.ErrorResponse("Документ не найден"));

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, document.OrganizationId))
                return Forbid();

            // Преобразуем строковый статус в enum
            if (Enum.TryParse<DocumentType>(status, true, out var documentStatus))
            {
                document.Status = documentStatus.ToString();
                //document.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return Ok(ApiResponse<Document>.SuccessResponse(document, "Статус документа обновлен"));
            }
            else
            {
                return BadRequest(ApiResponse<Document>.ErrorResponse("Недопустимый статус документа"));
            }
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

        private async Task<string> GenerateDocumentNumberAsync(int organizationId, DocumentType type)
        {
            var prefix = type switch
            {
                DocumentType.Receipt => "ПРИХ",
                DocumentType.WriteOff => "СПИС", 
                DocumentType.Transfer => "ПЕР",
                DocumentType.Inventory => "ИНВ",
                _ => "ДОК"
            };

            var today = DateTime.Today;
            var count = await _context.Documents
                .CountAsync(d => d.OrganizationId == organizationId && 
                                d.Type == type && 
                                d.CreatedAt.Year == today.Year);

            return $"{prefix}-{today:yyyyMM}-{count + 1:000}";
        }
    }
    public class AddProductRequest
    {
        public int DocumentId { get; set; }
        public required string ProductBarcode { get; set; }
        public double Quantity { get; set; } = 1.0;
    }
}