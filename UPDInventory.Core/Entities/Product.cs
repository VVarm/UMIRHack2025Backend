using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(100)]
        public string? Barcode { get; set; }
        
        public string? Description { get; set; }
        
        [MaxLength(50)]
        public string Unit { get; set; } = "шт."; // Добавляем свойство Unit
        
        public bool IsActive { get; set; } = true; // Добавляем свойство IsActive
        
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public ICollection<DocumentItem> DocumentItems { get; set; } = new List<DocumentItem>();
    }
}