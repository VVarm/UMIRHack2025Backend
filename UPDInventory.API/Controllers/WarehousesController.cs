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
    public class WarehousesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOrganizationService _organizationService;

        public WarehousesController(AppDbContext context, IOrganizationService organizationService)
        {
            _context = context;
            _organizationService = organizationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Warehouse>>> GetWarehouses([FromQuery] int? organizationId)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            IQueryable<Warehouse> query = _context.Warehouses;

            // Если указан organizationId, фильтруем по нему и проверяем доступ
            if (organizationId.HasValue)
            {
                if (!await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, organizationId.Value))
                    return Forbid();

                query = query.Where(w => w.OrganizationId == organizationId.Value);
            }
            else
            {
                // Если organizationId не указан, возвращаем склады всех организаций пользователя
                var userOrganizations = await _organizationService.GetUserOrganizationsAsync(userId.Value);
                var organizationIds = userOrganizations.Select(o => o.Id);
                query = query.Where(w => organizationIds.Contains(w.OrganizationId));
            }

            var warehouses = await query
                .Include(w => w.Organization)
                .ToListAsync();

            return Ok(warehouses);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Warehouse>> GetWarehouse(int id)
        {
            var warehouse = await _context.Warehouses
                .Include(w => w.Organization)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (warehouse == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, warehouse.OrganizationId))
                return Forbid();

            return Ok(warehouse);
        }

        [HttpPost]
        public async Task<ActionResult<Warehouse>> CreateWarehouse(WarehouseCreateDto warehouseDto)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, warehouseDto.OrganizationId))
                return Forbid();

            // Создаем Warehouse из DTO
            var warehouse = new Warehouse
            {
                Name = warehouseDto.Name,
                Address = warehouseDto.Address,
                IsActive = warehouseDto.IsActive,
                OrganizationId = warehouseDto.OrganizationId
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetWarehouse), new { id = warehouse.Id }, warehouse);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductPartialUpdateDto productDto)
        {
            if (id != productDto.Id)
                return BadRequest();

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, productDto.OrganizationId))
                return Forbid();

            // Обновляем только переданные поля
            if (!string.IsNullOrEmpty(productDto.Name))
                existingProduct.Name = productDto.Name;
            
            if (productDto.Barcode != null)
            {
                // Проверяем уникальность штрих-кода если он изменился
                if (productDto.Barcode != existingProduct.Barcode)
                {
                    var duplicateProduct = await _context.Products
                        .FirstOrDefaultAsync(p => p.OrganizationId == productDto.OrganizationId && 
                                                p.Barcode == productDto.Barcode && 
                                                p.Id != id);
                    
                    if (duplicateProduct != null)
                    {
                        return BadRequest(new { message = "Товар с таким штрих-кодом уже существует в организации" });
                    }
                }
                existingProduct.Barcode = productDto.Barcode;
            }
            
            if (productDto.Description != null)
                existingProduct.Description = productDto.Description;
            
            if (!string.IsNullOrEmpty(productDto.Unit))
                existingProduct.Unit = productDto.Unit;
            
            if (productDto.IsActive.HasValue)
                existingProduct.IsActive = productDto.IsActive.Value;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWarehouse(int id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, warehouse.OrganizationId))
                return Forbid();

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();

            return NoContent();
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