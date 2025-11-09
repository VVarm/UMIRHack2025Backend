using UPDInventory.Core.Entities;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace UPDInventory.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _organizationRepository;
        private readonly AppDbContext _context;

        public OrganizationService(IOrganizationRepository organizationRepository, AppDbContext context)
        {
            _organizationRepository = organizationRepository;
            _context = context;
        }

        public async Task<IEnumerable<Organization>> GetUserOrganizationsAsync(int userId)
        {
            return await _organizationRepository.GetUserOrganizationsAsync(userId);
        }

        public async Task<Organization> GetOrganizationByIdAsync(int id)
        {
            return await _organizationRepository.GetByIdAsync(id);
        }

        public async Task<Organization> CreateOrganizationAsync(Organization organization, int userId)
        {
            // Проверяем, существует ли организация с таким именем
            var exists = await _organizationRepository.OrganizationExistsAsync(organization.Name);
            if (exists)
            {
                throw new InvalidOperationException($"Организация с именем '{organization.Name}' уже существует");
            }

            // Добавляем организацию
            await _organizationRepository.AddAsync(organization);
            await _context.SaveChangesAsync();

            // Создаем связь пользователя с организацией
            var userOrganization = new UserOrganization
            {
                UserId = userId,
                OrganizationId = organization.Id,
                Role = "owner", // Владелец организации
                CreatedAt = DateTime.UtcNow
            };

            _context.UserOrganizations.Add(userOrganization);
            await _context.SaveChangesAsync();
            
            return organization;
        }

        public async Task UpdateOrganizationAsync(Organization organization)
        {
            _organizationRepository.Update(organization);
            await _context.SaveChangesAsync(); // ← Сохраняем через DbContext
        }

        public async Task<bool> DeleteOrganizationAsync(int id)
        {
            var organization = await _organizationRepository.GetByIdAsync(id);
            if (organization == null)
                return false;

            _organizationRepository.Remove(organization);
            await _context.SaveChangesAsync(); // ← Сохраняем через DbContext
            return true;
        }

        public async Task<bool> UserHasAccessToOrganizationAsync(int userId, int organizationId)
        {
            var userOrganizations = await _organizationRepository.GetUserOrganizationsAsync(userId);
            return userOrganizations.Any(org => org.Id == organizationId);
        }
    }
}