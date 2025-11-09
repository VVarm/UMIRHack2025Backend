using System;
using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.Entities
{
    public class UserOrganization
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = string.Empty; // 'storekeeper', 'auditor', 'admin'
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}