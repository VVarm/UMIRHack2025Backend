namespace UPDInventory.Core.Exceptions
{
    public class BusinessException : Exception
    {
        public string Code { get; }
        public int StatusCode { get; }

        public BusinessException(string message, string code = "BUSINESS_ERROR", int statusCode = 400) 
            : base(message)
        {
            Code = code;
            StatusCode = statusCode;
        }

        public BusinessException(string message, Exception innerException, string code = "BUSINESS_ERROR", int statusCode = 400)
            : base(message, innerException)
        {
            Code = code;
            StatusCode = statusCode;
        }
    }
}