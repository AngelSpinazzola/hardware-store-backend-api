using System.Text.Json;
using HardwareStore.Domain.Exceptions;
using Serilog;

namespace HardwareStore.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
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

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                ArgumentException ex => (StatusCodes.Status400BadRequest, ex.Message),
                InvalidOperationException ex => (StatusCodes.Status400BadRequest, ex.Message),
                NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
                ForbiddenException ex => (StatusCodes.Status403Forbidden, ex.Message),
                UnauthorizedAccessException ex => (StatusCodes.Status401Unauthorized, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };

            // Log según severidad
            if (statusCode >= 500)
            {
                Log.Error(exception, "Error no controlado en {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            }
            else
            {
                Log.Warning("Excepción controlada en {Method} {Path}: {Message}",
                    context.Request.Method, context.Request.Path, exception.Message);
            }

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var response = new { message };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
        }
    }
}