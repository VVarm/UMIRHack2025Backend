using UPDInventory.Core.Entities;
using UPDInventory.Core.Enums;

namespace UPDInventory.Core.Interfaces
{
    public interface IDocumentService
    {
        Task<Document> CreateDocumentAsync(Document document);
        Task<Document> GetDocumentWithItemsAsync(int documentId);
        Task<DocumentItem> AddItemToDocumentAsync(int documentId, DocumentItem item);
        Task<DocumentItem> UpdateDocumentItemAsync(int itemId, decimal quantityActual);
        Task<bool> CompleteDocumentAsync(int documentId);
        Task<bool> CancelDocumentAsync(int documentId);
        Task<DocumentItem> ScanAndAddProductAsync(int documentId, string barcode, decimal quantity);
        Task<IEnumerable<Document>> GetOrganizationDocumentsAsync(int organizationId, DocumentType? type = null);
    }
}