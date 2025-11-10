using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;
using UPDInventory.Core.DTOs;
using System.Security.Claims;

namespace UPDInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOrganizationService _organizationService;

        public ProductsController(AppDbContext context, IOrganizationService organizationService)
        {
            _context = context;
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] int? organizationId)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            IQueryable<Product> query = _context.Products;

            // Если указан organizationId, фильтруем по нему и проверяем доступ
            if (organizationId.HasValue)
            {
                if (!await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, organizationId.Value))
                    return Forbid();

                query = query.Where(p => p.OrganizationId == organizationId.Value);
            }
            else
            {
                // Если organizationId не указан, возвращаем товары всех организаций пользователя
                var userOrganizations = await _organizationService.GetUserOrganizationsAsync(userId.Value);
                var organizationIds = userOrganizations.Select(o => o.Id);
                query = query.Where(p => organizationIds.Contains(p.OrganizationId));
            }

            var products = await query
                .Include(p => p.Organization)
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Organization)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, product.OrganizationId))
                return Forbid();

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(ProductCreateDto productDto)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, productDto.OrganizationId))
                return Forbid();

            // Проверяем уникальность штрих-кода в организации
            if (!string.IsNullOrEmpty(productDto.Barcode))
            {
                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.OrganizationId == productDto.OrganizationId && p.Barcode == productDto.Barcode);
                
                if (existingProduct != null)
                {
                    return BadRequest(new { message = "Товар с таким штрих-кодом уже существует в организации" });
                }
            }

            // Создаем Product из DTO
            var product = new Product
            {
                Name = productDto.Name,
                Barcode = productDto.Barcode,
                Description = productDto.Description,
                Unit = productDto.Unit,
                IsActive = productDto.IsActive,
                OrganizationId = productDto.OrganizationId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Загружаем организацию для ответа
            await _context.Entry(product)
                .Reference(p => p.Organization)
                .LoadAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, product.OrganizationId))
                return Forbid();

            // Проверяем уникальность штрих-кода в организации (если изменился)
            if (!string.IsNullOrEmpty(product.Barcode) && product.Barcode != existingProduct.Barcode)
            {
                var duplicateProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.OrganizationId == product.OrganizationId && p.Barcode == product.Barcode && p.Id != id);
                
                if (duplicateProduct != null)
                {
                    return BadRequest(new { message = "Товар с таким штрих-кодом уже существует в организации" });
                }
            }

            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Barcode = product.Barcode;
            existingProduct.Unit = product.Unit;
            existingProduct.IsActive = product.IsActive;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, product.OrganizationId))
                return Forbid();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("barcode/{barcode}")]
        public async Task<ActionResult<ApiResponse<Product>>> GetProductByBarcode(string barcode, [FromQuery] int organizationId)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<Product>.ErrorResponse("Неавторизованный доступ"));

            if (!await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, organizationId))
                return Forbid();

            var product = await _context.Products
                .Include(p => p.Organization)
                .Where(p => p.OrganizationId == organizationId && p.Barcode == barcode)
                .Select(p => new Product
                {
                    Id = p.Id,
                    Name = p.Name,
                    Barcode = p.Barcode,
                    Description = p.Description,
                    OrganizationId = p.OrganizationId,
                    Unit = p.Unit,
                    IsActive = p.IsActive,
                    Organization = new Organization { Id = p.Organization.Id, Name = p.Organization.Name }
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(ApiResponse<Product>.ErrorResponse("Товар с указанным штрих-кодом не найден"));
            }

            return Ok(ApiResponse<Product>.SuccessResponse(product));
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