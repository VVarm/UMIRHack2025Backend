using Microsoft.EntityFrameworkCore;
using UPDInventory.Core.Entities;
using UPDInventory.Core.Enums;
using UPDInventory.Core.Interfaces;
using UPDInventory.Core.Data;
using Microsoft.Extensions.Logging;

namespace UPDInventory.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(AppDbContext context, ILogger<DocumentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Document> CreateDocumentAsync(Document document)
        {
            // Генерируем номер документа если не указан
            if (string.IsNullOrEmpty(document.Number))
            {
                document.Number = await GenerateDocumentNumberAsync(document.OrganizationId, document.Type);
            }

            // Устанавливаем статус "черновик" если не указан
            if (string.IsNullOrEmpty(document.Status))
            {
                document.Status = "draft";
            }

            document.CreatedAt = DateTime.UtcNow;

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Создан документ {DocumentNumber} типа {DocumentType}", document.Number, document.Type);

            return document;
        }

        public async Task<Document> GetDocumentWithItemsAsync(int documentId)
        {
            return await _context.Documents
                .Include(d => d.Organization)
                .Include(d => d.SourceWarehouse)
                .Include(d => d.DestinationWarehouse)
                .Include(d => d.Warehouse)
                .Include(d => d.Items)
                    .ThenInclude(i => i.Product)
                .Include(d => d.CreatedBy)
                .FirstOrDefaultAsync(d => d.Id == documentId);
        }

        public async Task<DocumentItem> AddItemToDocumentAsync(int documentId, DocumentItem item)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new InvalidOperationException("Документ не найден");
            }

            // Проверяем, что документ в статусе черновика
            if (document.Status != "draft")
            {
                throw new InvalidOperationException("Нельзя добавлять позиции в проведенный документ");
            }

            // Проверяем, что товар принадлежит той же организации
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null || product.OrganizationId != document.OrganizationId)
            {
                throw new InvalidOperationException("Товар не найден или принадлежит другой организации");
            }

            item.DocumentId = documentId;
            item.CreatedAt = DateTime.UtcNow;

            _context.DocumentItems.Add(item);
            await _context.SaveChangesAsync();

            // Загружаем связанные данные
            await _context.Entry(item)
                .Reference(i => i.Product)
                .LoadAsync();

            _logger.LogInformation("Добавлена позиция {ProductId} в документ {DocumentId}", item.ProductId, documentId);

            return item;
        }

        public async Task<DocumentItem> UpdateDocumentItemAsync(int itemId, decimal quantityActual)
        {
            var item = await _context.DocumentItems
                .Include(i => i.Document)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                throw new InvalidOperationException("Позиция документа не найдена");
            }

            // Проверяем, что документ в статусе черновика
            if (item.Document.Status != "draft")
            {
                throw new InvalidOperationException("Нельзя изменять позиции проведенного документа");
            }

            item.QuantityActual = quantityActual;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Обновлено количество для позиции {ItemId}: {QuantityActual}", itemId, quantityActual);

            return item;
        }

        public async Task<bool> CompleteDocumentAsync(int documentId)
        {
            var document = await _context.Documents
                .Include(d => d.Items)
                .FirstOrDefaultAsync(d => d.Id == documentId);

            if (document == null)
            {
                throw new InvalidOperationException("Документ не найден");
            }

            if (document.Status != "draft")
            {
                throw new InvalidOperationException("Документ уже проведен или отменен");
            }

            // Проверяем, что есть хотя бы одна позиция
            if (!document.Items.Any())
            {
                throw new InvalidOperationException("Нельзя провести документ без позиций");
            }

            // Для инвентаризации проверяем, что все количества заполнены
            if (document.Type == DocumentType.Inventory)
            {
                var incompleteItems = document.Items.Where(i => i.QuantityActual == 0).ToList();
                if (incompleteItems.Any())
                {
                    throw new InvalidOperationException($"Не все количества заполнены. Позиций без количества: {incompleteItems.Count}");
                }
            }

            document.Status = "completed";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Документ {DocumentId} проведен успешно", documentId);

            return true;
        }

        public async Task<bool> CancelDocumentAsync(int documentId)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new InvalidOperationException("Документ не найден");
            }

            if (document.Status != "draft")
            {
                throw new InvalidOperationException("Можно отменять только документы в статусе черновика");
            }

            document.Status = "cancelled";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Документ {DocumentId} отменен", documentId);

            return true;
        }

        public async Task<DocumentItem> ScanAndAddProductAsync(int documentId, string barcode, decimal quantity)
        {
            var document = await _context.Documents.FindAsync(documentId);
            if (document == null)
            {
                throw new InvalidOperationException("Документ не найден");
            }

            // Ищем товар по штрих-коду в организации
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.OrganizationId == document.OrganizationId && p.Barcode == barcode);

            if (product == null)
            {
                throw new InvalidOperationException($"Товар со штрих-кодом {barcode} не найден в организации");
            }

            // Проверяем, не добавлен ли уже этот товар в документ
            var existingItem = await _context.DocumentItems
                .FirstOrDefaultAsync(i => i.DocumentId == documentId && i.ProductId == product.Id);

            if (existingItem != null)
            {
                // Обновляем количество существующей позиции
                existingItem.QuantityActual += quantity;
                await _context.SaveChangesAsync();

                await _context.Entry(existingItem)
                    .Reference(i => i.Product)
                    .LoadAsync();

                _logger.LogInformation("Обновлено количество для товара {ProductId} в документе {DocumentId}", product.Id, documentId);

                return existingItem;
            }
            else
            {
                // Создаем новую позицию
                var newItem = new DocumentItem
                {
                    DocumentId = documentId,
                    ProductId = product.Id,
                    QuantityExpected = 0,
                    QuantityActual = quantity,
                    CreatedAt = DateTime.UtcNow
                };

                _context.DocumentItems.Add(newItem);
                await _context.SaveChangesAsync();

                await _context.Entry(newItem)
                    .Reference(i => i.Product)
                    .LoadAsync();

                _logger.LogInformation("Добавлен новый товар {ProductId} в документ {DocumentId}", product.Id, documentId);

                return newItem;
            }
        }

        public async Task<IEnumerable<Document>> GetOrganizationDocumentsAsync(int organizationId, DocumentType? type = null)
        {
            var query = _context.Documents
                .Include(d => d.Items)
                .ThenInclude(i => i.Product)
                .Where(d => d.OrganizationId == organizationId);

            if (type.HasValue)
            {
                query = query.Where(d => d.Type == type.Value);
            }

            return await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        private async Task<string> GenerateDocumentNumberAsync(int organizationId, DocumentType type)
        {
            var prefix = type switch
            {
                DocumentType.Receipt => "ПРИХ",
                DocumentType.WriteOff => "СПИС",
                DocumentType.Transfer => "ПЕР",
                DocumentType.Inventory => "ИНВ",
                _ => "ДОК"
            };

            var today = DateTime.Today;
            var year = today.Year % 100;
            var month = today.Month;

            var lastDocument = await _context.Documents
                .Where(d => d.OrganizationId == organizationId && d.Type == type && d.CreatedAt.Year == today.Year)
                .OrderByDescending(d => d.Number)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastDocument != null && !string.IsNullOrEmpty(lastDocument.Number))
            {
                var parts = lastDocument.Number.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNum))
                {
                    nextNumber = lastNum + 1;
                }
            }

            return $"{prefix}-{year:00}{month:00}-{nextNumber:0000}";
        }
    }
}