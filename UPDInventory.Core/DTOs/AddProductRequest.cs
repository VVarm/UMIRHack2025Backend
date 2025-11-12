namespace UPDInventory.Core.DTOs
{
    public class AddProductRequest
    {
        public int DocumentId { get; set; }
        public string ProductBarcode { get; set; } = string.Empty;
        public double Quantity { get; set; } = 1.0;
    }
}