namespace UPDInventory.Core.Exceptions
{
    public class AccessDeniedException : BusinessException
    {
        public AccessDeniedException(string message) 
            : base(message, "ACCESS_DENIED", 403)
        {
        }
    }
}