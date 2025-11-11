using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.DTOs
{
    public class WarehouseCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        public string? Address { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        public int OrganizationId { get; set; }
    }
}