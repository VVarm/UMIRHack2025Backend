namespace UPDInventory.Core.DTOs
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public int OrganizationId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}