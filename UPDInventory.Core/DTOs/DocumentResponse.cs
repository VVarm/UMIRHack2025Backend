namespace UPDInventory.Core.DTOs
{
    public class DocumentResponse
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // 'draft', 'in_progress', 'completed', 'cancelled'
        public string DateCreated { get; set; } = string.Empty;
        public string DocumentDate { get; set; } = string.Empty;
        public int OrganizationId { get; set; }
        public int WarehouseId { get; set; }
        public int CreatedById { get; set; }
        public WarehouseDto? Warehouse { get; set; }
        public List<DocumentItemResponse> Items { get; set; } = new List<DocumentItemResponse>();
    }
}