using System;
using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.Entities
{
    public class DocumentItem
    {
        [Key]
        public int Id { get; set; }
        
        public int DocumentId { get; set; }
        public Document Document { get; set; } = null!;
        
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        
        public decimal QuantityExpected { get; set; } = 0;
        
        public decimal QuantityActual { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}