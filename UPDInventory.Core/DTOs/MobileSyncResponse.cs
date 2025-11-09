using UPDInventory.Core.Entities;

namespace UPDInventory.Core.DTOs
{
    public class MobileSyncResponse
    {
        public Organization Organization { get; set; } = null!;
        public List<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Document> Documents { get; set; } = new List<Document>();
        public DateTime SyncTimestamp { get; set; } = DateTime.UtcNow;
        public string SessionToken { get; set; } = string.Empty;
    }
}