using System.ComponentModel.DataAnnotations;
using UPDInventory.Core.Enums;

namespace UPDInventory.Core.DTOs
{
    public class DocumentCreateDto
    {
        [Required]
        public DocumentType Type { get; set; }
        
        public string? Number { get; set; }
        public string? Comment { get; set; }
        
        [Required]
        public DateTime DocumentDate { get; set; }
        
        [Required]
        public int OrganizationId { get; set; }
        
        public int? SourceWarehouseId { get; set; }
        public int? DestinationWarehouseId { get; set; }
        public int? WarehouseId { get; set; }
    }
}