using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.DTOs
{
    public class DocumentUpdateDto
    {
        [Required]
        public int Id { get; set; }
        
        public string? Comment { get; set; }
        
        public string? Status { get; set; }
    }
}