using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.DTOs
{
    public class ProductCreateDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Barcode { get; set; }
        
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string Unit { get; set; } = "шт.";
        
        public bool IsActive { get; set; } = true;
        
        [Required]
        public int OrganizationId { get; set; }
    }
}