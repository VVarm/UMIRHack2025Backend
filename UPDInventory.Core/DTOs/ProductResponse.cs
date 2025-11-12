namespace UPDInventory.Core.DTOs
{
    public class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? Description { get; set; }
        public int OrganizationId { get; set; }
        public string Unit { get; set; } = "шт.";
        public bool IsActive { get; set; } = true;
    }
}