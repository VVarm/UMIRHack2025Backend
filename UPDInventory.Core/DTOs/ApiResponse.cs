using System.Text.Json.Serialization;

namespace UPDInventory.Core.DTOs
{
    public class ApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public T Data { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        public ApiResponse(bool success, T data = default, string message = null, string error = null)
        {
            Success = success;
            Data = data;
            Message = message;
            Error = error;
        }

        public static ApiResponse<T> SuccessResponse(T data, string message = null)
        {
            return new ApiResponse<T>(true, data, message);
        }

        public static ApiResponse<T> ErrorResponse(string error, string message = null)
        {
            return new ApiResponse<T>(false, default, message, error);
        }
    }

    public class ApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }

        public ApiResponse(bool success, string message = null, string error = null)
        {
            Success = success;
            Message = message;
            Error = error;
        }

        public static ApiResponse SuccessResponse(string message = null)
        {
            return new ApiResponse(true, message);
        }

        public static ApiResponse ErrorResponse(string error, string message = null)
        {
            return new ApiResponse(false, message, error);
        }
    }
}