namespace UPDInventory.Core.Exceptions
{
    public class NotFoundException : BusinessException
    {
        public NotFoundException(string message) 
            : base(message, "NOT_FOUND", 404)
        {
        }

        public NotFoundException(string entityName, int id)
            : base($"{entityName} с ID {id} не найден", "NOT_FOUND", 404)
        {
        }
    }
}