using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Utils.Exceptions;
using Utils.DTO.Api;

namespace mediTool.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
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
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var statusCode = HttpStatusCode.InternalServerError;
            var message = "Ocurrió un error inesperado en el servidor.";
            var details = new List<string>();

            switch (exception)
            {
                case InternalError internalEx:
                    statusCode = HttpStatusCode.InternalServerError;
                    message = "Error interno del servidor.";
                    if (_env.IsDevelopment())
                    {
                        details.Add(internalEx.Message);
                        details.AddRange(internalEx.Details);
                    }
                    break;
                case DbUpdateException:
                    statusCode = HttpStatusCode.Conflict;
                    message = "No se puede eliminar el recurso porque tiene registros asociados.";
                    if (_env.IsDevelopment())
                    {
                        details.Add(exception.Message);
                    }
                    break;
                case AppException appEx:
                    statusCode = appEx.StatusCode;
                    message = appEx.Message;
                    details = appEx.Details;
                    break;
                case KeyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    message = "El recurso solicitado no existe.";
                    if (_env.IsDevelopment())
                    {
                        details.Add(exception.Message);
                    }
                    break;
                case ArgumentException or InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    message = "La solicitud no es válida.";
                    if (_env.IsDevelopment())
                    {
                        details.Add(exception.Message);
                    }
                    break;
                case UnauthorizedAccessException:
                    statusCode = HttpStatusCode.Unauthorized;
                    message = "No está autorizado para acceder a este recurso.";
                    if (_env.IsDevelopment())
                    {
                        details.Add(exception.Message);
                    }
                    break;
                default:
                    if (_env.IsDevelopment())
                    {
                        details.Add(exception.Message);
                        if (exception.StackTrace != null)
                        {
                            details.Add(exception.StackTrace);
                        }
                    }
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new ApiErrorResponseDto
            {
                Status = "error",
                Code = (int)statusCode,
                Message = message,
                Details = details
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }
    }
}


