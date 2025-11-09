using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UPDInventory.Core.Entities;
using UPDInventory.Core.DTOs;
using UPDInventory.Services.Interfaces;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;

namespace UPDInventory.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;

        public AuthController(ITokenService tokenService, AppDbContext context)
        {
            _tokenService = tokenService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            // Поиск пользователя по email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new  
                {  
                    Message = "Неверный email или пароль" 
                });
            }

            // Получение организаций пользователя
            var organizations = await _context.UserOrganizations
                .Where(uo => uo.UserId == user.Id)
                .Include(uo => uo.Organization)
                .Select(uo => new OrganizationRole
                {
                    OrganizationId = uo.Organization.Id,
                    OrganizationName = uo.Organization.Name,
                    Role = uo.Role
                })
                .ToListAsync();

            var token = _tokenService.GenerateToken(user, organizations);

            return Ok(new 
            {
                Token = token,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName
                }
            });
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            // Проверка существования пользователя
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new  
                {  
                    Message = "Пользователь с таким email уже существует" 
                });
            }

            // Создание нового пользователя
            var user = new User
            {
                Email = request.Email,
                FullName = request.Name,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Генерация токена (без организаций для нового пользователя)
            var token = _tokenService.GenerateToken(user, new List<OrganizationRole>());

            return Ok(new 
            {
                Message = "Регистрация успешна",
                Token = token,
                User = new UserInfo
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName
                }
            });
        }
    }

    public class RegisterRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string Name { get; set; }
    }
}