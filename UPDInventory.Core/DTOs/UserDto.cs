namespace UPDInventory.Core.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }
}