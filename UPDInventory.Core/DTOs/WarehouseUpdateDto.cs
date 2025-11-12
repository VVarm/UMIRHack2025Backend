using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.DTOs
{
    public class WarehouseUpdateDto
    {
        [Required]
        public int Id { get; set; }
        
        [MaxLength(255)]
        public string? Name { get; set; }
        
        public string? Address { get; set; }
        
        public bool? IsActive { get; set; }
    }
}