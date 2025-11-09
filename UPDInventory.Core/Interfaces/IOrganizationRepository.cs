using UPDInventory.Core.Entities;

namespace UPDInventory.Core.Interfaces
{
    public interface IOrganizationRepository : IGenericRepository<Organization>
    {
        Task<IEnumerable<Organization>> GetUserOrganizationsAsync(int userId);
        Task<Organization> GetOrganizationWithUsersAsync(int organizationId);
        Task<bool> OrganizationExistsAsync(string name);
    }
}