using System.Security.Claims;
using UPDInventory.Core.Interfaces;

namespace UPDInventory.API.Middleware
{
    public class OrganizationAccessMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<OrganizationAccessMiddleware> _logger;

        public OrganizationAccessMiddleware(RequestDelegate next, ILogger<OrganizationAccessMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IOrganizationService organizationService)
        {
            // Пропускаем запросы к аутентификации и публичным эндпоинтам
            if (context.Request.Path.StartsWithSegments("/api/auth") ||
                context.Request.Path.StartsWithSegments("/swagger") ||
                context.Request.Path.StartsWithSegments("/favicon.ico"))
            {
                await _next(context);
                return;
            }

            // Проверяем аутентификацию пользователя
            if (!context.User.Identity.IsAuthenticated)
            {
                await _next(context);
                return;
            }

            // Получаем organizationId из query string или route values
            var organizationId = GetOrganizationIdFromRequest(context);

            if (organizationId.HasValue)
            {
                var userId = GetUserIdFromClaims(context.User);
                
                if (userId.HasValue)
                {
                    var hasAccess = await organizationService.UserHasAccessToOrganizationAsync(userId.Value, organizationId.Value);
                    
                    if (!hasAccess)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsync("Доступ к организации запрещен");
                        return;
                    }
                }
            }

            await _next(context);
        }

        private int? GetOrganizationIdFromRequest(HttpContext context)
        {
            // Пытаемся получить organizationId из query string
            if (context.Request.Query.TryGetValue("organizationId", out var orgIdValue) &&
                int.TryParse(orgIdValue, out int orgId))
            {
                return orgId;
            }

            // Пытаемся получить из route values
            if (context.Request.RouteValues.TryGetValue("organizationId", out var routeOrgId) &&
                int.TryParse(routeOrgId?.ToString(), out int routeOrgIdInt))
            {
                return routeOrgIdInt;
            }

            return null;
        }

        private int? GetUserIdFromClaims(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            return null;
        }
    }
}