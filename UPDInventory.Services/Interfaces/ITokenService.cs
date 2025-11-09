using UPDInventory.Core.Entities;

namespace UPDInventory.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, List<Core.DTOs.OrganizationRole> organizations);
        bool ValidateToken(string token);
    }
}