namespace UPDInventory.Core.DTOs
{
    public class OrganizationResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Inn { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string UserRole { get; set; } = string.Empty; // 'storekeeper', 'auditor', 'admin', 'owner'
    }
}