using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UPDInventory.Core.DTOs;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;

namespace UPDInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int id))
            {
                return Unauthorized(ApiResponse<UserDto>.ErrorResponse("Неавторизованный доступ"));
            }

            var user = await _context.Users
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    CreatedAt = u.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse("Пользователь не найден"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
        public required string CreatedAt { get; set; }
    }
}