using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using UPDInventory.Core.Enums;

namespace UPDInventory.Core.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DocumentType Type { get; set; } // Используем enum вместо string
        
        [Required]
        [MaxLength(100)]
        public string Number { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "draft"; // 'draft', 'in_progress', 'completed', 'cancelled'
        
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Переименовано из DateCreated
        
        [Required]
        public DateTime DocumentDate { get; set; }
        
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        
        // Для перемещений: исходный и целевой склады
        public int? SourceWarehouseId { get; set; }
        public Warehouse? SourceWarehouse { get; set; }
        
        public int? DestinationWarehouseId { get; set; }
        public Warehouse? DestinationWarehouse { get; set; }
        
        // Для приходов/списаний: основной склад
        public int? WarehouseId { get; set; }
        public Warehouse? Warehouse { get; set; }
        
        public int CreatedByUserId { get; set; } // Переименовано из CreatedById
        public User CreatedBy { get; set; } = null!;
        
        // Навигационные свойства
        public ICollection<DocumentItem> Items { get; set; } = new List<DocumentItem>(); // Переименовано из DocumentItems
    }
}