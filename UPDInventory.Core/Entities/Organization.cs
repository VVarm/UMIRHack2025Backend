using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.Entities
{
    public class Organization
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string? Inn { get; set; } // ИНН (уникальный)
        
        public string? Address { get; set; }
        
        [MaxLength(50)]
        public string? Phone { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Навигационные свойства
        public ICollection<UserOrganization> UserOrganizations { get; set; } = new List<UserOrganization>();
        public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<Document> Documents { get; set; } = new List<Document>();
        public ICollection<MobileSession> MobileSessions { get; set; } = new List<MobileSession>();
    }
}