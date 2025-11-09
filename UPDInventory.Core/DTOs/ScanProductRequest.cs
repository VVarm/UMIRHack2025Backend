namespace UPDInventory.Core.DTOs
{
    public class ScanProductRequest
    {
        public string Barcode { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public int? WarehouseId { get; set; }
    }
}