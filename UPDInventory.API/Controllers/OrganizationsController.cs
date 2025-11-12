using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Interfaces;
using System.Security.Claims;
using UPDInventory.Core.DTOs;

namespace UPDInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Organization>>> GetMyOrganizations()
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized();

            var organizations = await _organizationService.GetUserOrganizationsAsync(userId.Value);
            return Ok(organizations);
        }

        [HttpGet("my/mobile")]
        public async Task<ActionResult<ApiResponse<List<OrganizationResponse>>>> GetMyOrganizationsMobile()
        {
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<List<OrganizationResponse>>.ErrorResponse("Неавторизованный доступ"));

            var organizations = await _organizationService.GetUserOrganizationsAsync(userId.Value);
            
            // Преобразуем в DTO
            var organizationDtos = organizations.Select(o => new OrganizationResponse
            {
                Id = o.Id,
                Name = o.Name,
                Inn = o.Inn,
                Address = o.Address,
                Phone = o.Phone,
                UserRole = "storekeeper" // TODO: получить реальную роль
            }).ToList();

            return Ok(ApiResponse<List<OrganizationResponse>>.SuccessResponse(organizationDtos));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Organization>> GetOrganization(int id)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            if (organization == null)
                return NotFound();

            // Проверяем доступ пользователя к организации
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, id))
                return Forbid();

            return Ok(organization);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Organization>> CreateOrganization(Organization organization)
        {
            try
            {
                var userId = GetUserIdFromClaims();
                if (!userId.HasValue)
                    return Unauthorized();

                var createdOrganization = await _organizationService.CreateOrganizationAsync(organization, userId.Value);
                return CreatedAtAction(nameof(GetOrganization), new { id = createdOrganization.Id }, createdOrganization);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrganization(int id, [FromBody] UpdateOrganizationRequest request)
        {
            // Проверяем доступ пользователя к организации
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, id))
                return Forbid();

            try
            {
                // Получаем текущую организацию
                var existingOrganization = await _organizationService.GetOrganizationByIdAsync(id);
                if (existingOrganization == null)
                    return NotFound();

                // Обновляем только переданные поля
                if (request.Name != null)
                    existingOrganization.Name = request.Name;
                if (request.Inn != null)
                    existingOrganization.Inn = request.Inn;
                if (request.Address != null)
                    existingOrganization.Address = request.Address;
                if (request.Phone != null)
                    existingOrganization.Phone = request.Phone;

                await _organizationService.UpdateOrganizationAsync(existingOrganization);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            // Проверяем доступ пользователя к организации
            var userId = GetUserIdFromClaims();
            if (!userId.HasValue || !await _organizationService.UserHasAccessToOrganizationAsync(userId.Value, id))
                return Forbid();

            var result = await _organizationService.DeleteOrganizationAsync(id);
            if (!result)
                return NotFound();

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