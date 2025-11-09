using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Interfaces
{
    public interface IOrganizationService
    {
        Task<IEnumerable<Organization>> GetUserOrganizationsAsync(int userId);
        Task<Organization> GetOrganizationByIdAsync(int id);
        Task<Organization> CreateOrganizationAsync(Organization organization, int userId);
        Task UpdateOrganizationAsync(Organization organization);
        Task<bool> DeleteOrganizationAsync(int id);
        Task<bool> UserHasAccessToOrganizationAsync(int userId, int organizationId);
    }
}