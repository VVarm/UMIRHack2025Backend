using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.DTOs
{
    public class AuthRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
        
        public string? Name { get; set; } // только для регистрации
    }
}