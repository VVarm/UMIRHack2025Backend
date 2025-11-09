using System.ComponentModel.DataAnnotations;

namespace UPDInventory.Core.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email является обязательным полем")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль является обязательным полем")]
        public string Password { get; set; } = string.Empty;
    }
}