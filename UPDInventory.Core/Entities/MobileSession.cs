using System;
using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.Entities
{
    public class MobileSession
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Token { get; set; } = string.Empty;
        
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        
        public int CreatedById { get; set; }
        public User CreatedBy { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; }
        
        public bool IsUsed { get; set; } = false;
    }
}