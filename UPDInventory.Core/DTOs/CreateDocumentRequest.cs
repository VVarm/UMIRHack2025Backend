namespace UPDInventory.Core.DTOs
{
    public class CreateDocumentRequest
    {
        public string Type { get; set; } = string.Empty; // "Inventory", "Receipt" и т.д.
        public string DocumentDate { get; set; } = string.Empty;
        public int WarehouseId { get; set; }
    }
}
