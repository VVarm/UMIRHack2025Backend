using System.Net;
using System.Text.Json;
using UPDInventory.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace UPDInventory.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            var errorResponse = new ErrorResponse
            {
                Success = false,
                Message = "Произошла ошибка при обработке запроса"
            };

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    response.StatusCode = notFoundEx.StatusCode;
                    errorResponse.Message = notFoundEx.Message;
                    errorResponse.Code = notFoundEx.Code;
                    _logger.LogWarning(notFoundEx, "Объект не найден: {Message}", notFoundEx.Message);
                    break;

                case AccessDeniedException accessEx:
                    response.StatusCode = accessEx.StatusCode;
                    errorResponse.Message = accessEx.Message;
                    errorResponse.Code = accessEx.Code;
                    _logger.LogWarning(accessEx, "Доступ запрещен: {Message}", accessEx.Message);
                    break;

                case BusinessException businessEx:
                    response.StatusCode = businessEx.StatusCode;
                    errorResponse.Message = businessEx.Message;
                    errorResponse.Code = businessEx.Code;
                    _logger.LogWarning(businessEx, "Бизнес-ошибка: {Message}", businessEx.Message);
                    break;

                case DbUpdateException dbEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "Ошибка при сохранении данных в базу";
                    errorResponse.Code = "DATABASE_ERROR";
                    _logger.LogError(dbEx, "Ошибка базы данных: {Message}", dbEx.Message);
                    break;

                case JsonException jsonEx:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorResponse.Message = "Ошибка в формате JSON данных";
                    errorResponse.Code = "JSON_ERROR";
                    _logger.LogWarning(jsonEx, "Ошибка JSON: {Message}", jsonEx.Message);
                    break;

                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorResponse.Message = _env.IsDevelopment() ? exception.Message : "Внутренняя ошибка сервера";
                    errorResponse.Code = "INTERNAL_ERROR";
                    _logger.LogError(exception, "Необработанное исключение: {Message}", exception.Message);
                    break;
            }

            // В режиме разработки добавляем детали исключения
            if (_env.IsDevelopment())
            {
                errorResponse.StackTrace = exception.StackTrace;
                errorResponse.Details = exception.ToString();
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(errorResponse, options);
            await response.WriteAsync(json);
        }
    }

    public class ErrorResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? Details { get; set; }
    }
}