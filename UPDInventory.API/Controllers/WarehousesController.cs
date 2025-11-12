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
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses([FromQuery] int? organizationId)
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            IQueryable<Warehouse> query = _context.Warehouses;

            if (organizationId.HasValue)
            {
                if (!await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, organizationId.Value))
                    return Forbid();
                query = query.Where(w => w.OrganizationId == organizationId.Value);
            }
            else
            {
                var userOrganizations = await _organizationService.GetUserOrganizationsAsync(userId.Value);
                var organizationIds = userOrganizations.Select(o => o.Id);
                query = query.Where(w => organizationIds.Contains(w.OrganizationId));
            }

            var warehouses = await query.ToListAsync();
            
            // Преобразуем в DTO
            var warehouseDtos = warehouses.Select(w => new WarehouseDto
            {
                Id = w.Id,
                Name = w.Name,
                Address = w.Address,
                IsActive = w.IsActive,
                OrganizationId = w.OrganizationId,
                CreatedAt = w.CreatedAt
            });

            return Ok(warehouseDtos);
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
        public async Task<IActionResult> UpdateWarehouse(int id, [FromBody] WarehouseUpdateDto warehouseDto)
        {
            if (id != warehouseDto.Id)
                return BadRequest();

            var existingWarehouse = await _context.Warehouses.FindAsync(id);
            if (existingWarehouse == null)
                return NotFound();

            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, existingWarehouse.OrganizationId))
                return Forbid();

            // Обновляем поля
            if (!string.IsNullOrEmpty(warehouseDto.Name))
                existingWarehouse.Name = warehouseDto.Name;
            
            if (warehouseDto.Address != null)
                existingWarehouse.Address = warehouseDto.Address;
            
            if (warehouseDto.IsActive.HasValue)
                existingWarehouse.IsActive = warehouseDto.IsActive.Value;

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