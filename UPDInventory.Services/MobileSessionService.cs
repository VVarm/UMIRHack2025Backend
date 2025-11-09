using Microsoft.EntityFrameworkCore;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace UPDInventory.Services
{
    public class MobileSessionService : IMobileSessionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MobileSessionService> _logger;

        public MobileSessionService(AppDbContext context, ILogger<MobileSessionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<MobileSession> CreateSessionAsync(int organizationId, int userId, TimeSpan? validityPeriod = null)
        {
            validityPeriod ??= TimeSpan.FromHours(24); // По умолчанию 24 часа

            var token = GenerateSecureToken();
            var session = new MobileSession
            {
                Token = token,
                OrganizationId = organizationId,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(validityPeriod.Value),
                IsUsed = false
            };

            _context.MobileSessions.Add(session);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Создана мобильная сессия для пользователя {UserId}, организации {OrganizationId}", userId, organizationId);

            return session;
        }

        public async Task<MobileSession> ValidateAndUseSessionAsync(string token)
        {
            var session = await _context.MobileSessions
                .Include(ms => ms.Organization)
                .Include(ms => ms.CreatedBy)
                .FirstOrDefaultAsync(ms => ms.Token == token);

            if (session == null)
            {
                throw new InvalidOperationException("Сессия не найдена");
            }

            if (session.IsUsed)
            {
                throw new InvalidOperationException("Сессия уже использована");
            }

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Срок действия сессии истек");
            }

            // Помечаем сессию как использованную
            session.IsUsed = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Мобильная сессия {Token} успешно использована", token);

            return session;
        }

        public async Task<bool> IsSessionValidAsync(string token)
        {
            var session = await _context.MobileSessions
                .FirstOrDefaultAsync(ms => ms.Token == token);

            return session != null && !session.IsUsed && session.ExpiresAt > DateTime.UtcNow;
        }

        public async Task<IEnumerable<MobileSession>> GetUserSessionsAsync(int userId)
        {
            return await _context.MobileSessions
                .Include(ms => ms.Organization)
                .Where(ms => ms.CreatedById == userId && ms.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(ms => ms.CreatedAt)
                .ToListAsync();
        }

        public async Task RevokeSessionAsync(string token)
        {
            var session = await _context.MobileSessions
                .FirstOrDefaultAsync(ms => ms.Token == token);

            if (session != null)
            {
                session.IsUsed = true; // Помечаем как использованную для отзыва
                await _context.SaveChangesAsync();

                _logger.LogInformation("Мобильная сессия {Token} отозвана", token);
            }
        }

        private string GenerateSecureToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}