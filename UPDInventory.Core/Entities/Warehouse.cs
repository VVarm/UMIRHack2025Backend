using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.Entities
{
    public class Warehouse
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        public string? Address { get; set; }
        
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}