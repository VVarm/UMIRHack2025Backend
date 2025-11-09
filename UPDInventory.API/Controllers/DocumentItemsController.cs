using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UPDInventory.Core.DTOs;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UPDInventory.API.Controllers
{
    [ApiController]
    [Route("api/documents/{documentId}/items")]
    [Authorize]
    public class DocumentItemsController : ControllerBase
    {
        private readonly IDocumentService _documentService;
        private readonly IOrganizationService _organizationService;
        private readonly AppDbContext _context;

        public DocumentItemsController(
            IDocumentService documentService,
            IOrganizationService organizationService,
            AppDbContext context)
        {
            _documentService = documentService;
            _organizationService = organizationService;
            _context = context;
        }

        [HttpPost]
        public async Task<ActionResult<DocumentItem>> AddItem(int documentId, DocumentItem item)
        {
            try
            {
                // Проверяем доступ к документу
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                    return NotFound();

                var userId = GetUserIdFromClaims();
                if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, document.OrganizationId))
                    return Forbid();

                var result = await _documentService.AddItemToDocumentAsync(documentId, item);
                return CreatedAtAction(nameof(GetItem), new { documentId, id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("scan")]
        public async Task<ActionResult<DocumentItem>> ScanProduct(int documentId, ScanProductRequest request)
        {
            try
            {
                // Проверяем доступ к документу
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                    return NotFound();

                var userId = GetUserIdFromClaims();
                if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, document.OrganizationId))
                    return Forbid();

                var result = await _documentService.ScanAndAddProductAsync(documentId, request.Barcode, request.Quantity);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DocumentItem>> UpdateItem(int documentId, int id, [FromBody] decimal quantityActual)
        {
            try
            {
                // Проверяем доступ к документу через позицию
                var item = await _context.DocumentItems
                    .Include(i => i.Document)
                    .FirstOrDefaultAsync(i => i.Id == id && i.DocumentId == documentId);

                if (item == null)
                    return NotFound();

                var userId = GetUserIdFromClaims();
                if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, item.Document.OrganizationId))
                    return Forbid();

                var result = await _documentService.UpdateDocumentItemAsync(id, quantityActual);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int documentId, int id)
        {
            try
            {
                // Проверяем доступ к документу через позицию
                var item = await _context.DocumentItems
                    .Include(i => i.Document)
                    .FirstOrDefaultAsync(i => i.Id == id && i.DocumentId == documentId);

                if (item == null)
                    return NotFound();

                var userId = GetUserIdFromClaims();
                if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, item.Document.OrganizationId))
                    return Forbid();

                // Проверяем, что документ в статусе черновика
                if (item.Document.Status != "draft")
                {
                    return BadRequest(new { message = "Нельзя удалять позиции из проведенного документа" });
                }

                _context.DocumentItems.Remove(item);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DocumentItem>> GetItem(int documentId, int id)
        {
            var item = await _context.DocumentItems
                .Include(i => i.Product)
                .Include(i => i.Document)
                .FirstOrDefaultAsync(i => i.Id == id && i.DocumentId == documentId);

            if (item == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, item.Document.OrganizationId))
                return Forbid();

            return Ok(item);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteDocument(int documentId)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                    return NotFound();

                var userId = GetUserIdFromClaims();
                if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, document.OrganizationId))
                    return Forbid();

                var result = await _documentService.CompleteDocumentAsync(documentId);
                return Ok(new { message = "Документ успешно проведен" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("cancel")]
        public async Task<IActionResult> CancelDocument(int documentId)
        {
            try
            {
                var document = await _context.Documents.FindAsync(documentId);
                if (document == null)
                    return NotFound();

                var userId = GetUserIdFromClaims();
                if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, document.OrganizationId))
                    return Forbid();

                var result = await _documentService.CancelDocumentAsync(documentId);
                return Ok(new { message = "Документ успешно отменен" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
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
    }
}