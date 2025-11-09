using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Interfaces
{
    public interface IMobileSessionService
    {
        Task<MobileSession> CreateSessionAsync(int organizationId, int userId, TimeSpan? validityPeriod = null);
        Task<MobileSession> ValidateAndUseSessionAsync(string token);
        Task<bool> IsSessionValidAsync(string token);
        Task<IEnumerable<MobileSession>> GetUserSessionsAsync(int userId);
        Task RevokeSessionAsync(string token);
    }
}