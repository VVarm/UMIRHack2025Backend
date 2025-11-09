using Microsoft.EntityFrameworkCore;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;

namespace UPDInventory.Infrastructure.Data
{
    public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Organization>> GetUserOrganizationsAsync(int userId)
        {
            return await _context.Organizations
                .Include(o => o.UserOrganizations)
                .Where(o => o.UserOrganizations.Any(uo => uo.UserId == userId))
                .ToListAsync();
        }

        public async Task<Organization> GetOrganizationWithUsersAsync(int organizationId)
        {
            return await _context.Organizations
                .Include(o => o.UserOrganizations)
                .ThenInclude(uo => uo.User)
                .FirstOrDefaultAsync(o => o.Id == organizationId);
        }

        public async Task<bool> OrganizationExistsAsync(string name)
        {
            return await _context.Organizations
                .AnyAsync(o => o.Name.ToLower() == name.ToLower());
        }
    }
}