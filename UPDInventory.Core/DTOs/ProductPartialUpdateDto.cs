using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.DTOs
{
    public class ProductPartialUpdateDto
    {
        [Required]
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string? Name { get; set; }
        
        [MaxLength(100)]
        public string? Barcode { get; set; }
        
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string? Unit { get; set; }
        
        public bool? IsActive { get; set; }
        
        [Required]
        public int OrganizationId { get; set; }
    }
}