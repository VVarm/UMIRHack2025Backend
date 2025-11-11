using UPDInventory.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.DTOs
{
    public class DocumentCreateDto
    {
        [Required]
        public DocumentType Type { get; set; }
        
        [Required]
        public int OrganizationId { get; set; }
        
        public string Number { get; set; } = string.Empty;
        
        [Required]
        public DateTime DocumentDate { get; set; } = DateTime.UtcNow;
        
        public string? Comment { get; set; }
        
        // Для приходов/списаний
        public int? WarehouseId { get; set; }
        
        // Для перемещений
        public int? SourceWarehouseId { get; set; }
        public int? DestinationWarehouseId { get; set; }
    }
}