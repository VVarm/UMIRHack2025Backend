namespace UPDInventory.Core.DTOs
{
    public class DocumentItemResponse
    {
        public int Id { get; set; }
        public int DocumentId { get; set; }
        public int ProductId { get; set; }
        public double QuantityExpected { get; set; }
        public double QuantityActual { get; set; }
        public ProductResponse? Product { get; set; }
    }
}