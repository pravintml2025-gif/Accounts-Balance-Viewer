using System.Net;
using System.Text.Json;
using Adra.Core.Exceptions;

namespace Adra.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, details) = exception switch
        {
            AccountNotFoundException accountNotFound => (
                HttpStatusCode.NotFound, 
                accountNotFound.Message, 
                "The requested account could not be found."
            ),
            
            DuplicateAccountException duplicateAccount => (
                HttpStatusCode.Conflict, 
                duplicateAccount.Message, 
                "An account with this name already exists."
            ),
            
            InvalidFileFormatException fileFormat => (
                HttpStatusCode.BadRequest, 
                fileFormat.Message, 
                "The uploaded file format is invalid or corrupted."
            ),
            
            BusinessRuleViolationException businessRule => (
                HttpStatusCode.BadRequest, 
                businessRule.Message, 
                "A business rule was violated."
            ),
            
            UnauthorizedOperationException unauthorized => (
                HttpStatusCode.Forbidden, 
                unauthorized.Message, 
                "You don't have permission to perform this operation."
            ),
            
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized, 
                "Unauthorized access", 
                "Authentication is required to access this resource."
            ),
            
            ArgumentException argument => (
                HttpStatusCode.BadRequest, 
                argument.Message, 
                "One or more arguments are invalid."
            ),
            
            OperationCanceledException => (
                HttpStatusCode.RequestTimeout, 
                "The operation was cancelled", 
                "The request timed out or was cancelled."
            ),
            
            DomainException domain => (
                HttpStatusCode.BadRequest, 
                domain.Message, 
                "A domain-specific error occurred."
            ),
            
            _ => (
                HttpStatusCode.InternalServerError, 
                "An internal server error occurred", 
                "An unexpected error occurred while processing your request."
            )
        };

        var response = new
        {
            success = false,
            message = message,
            details = details,
            traceId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = (int)statusCode;
        
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
        
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
