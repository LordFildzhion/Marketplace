
using System.Net;
using System.Text.Json;
using Marketplace.Application.Common.Exceptions;
using Marketplace.Web.Models.Responses;

namespace Marketplace.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception occurred");

        var statusCode = exception switch
        {
            NotFoundException _ => HttpStatusCode.NotFound,
            ValidationException _ => HttpStatusCode.BadRequest,
            UnauthorizedException _ => HttpStatusCode.Unauthorized,
            ForbiddenException _ => HttpStatusCode.Forbidden,
            ConflictException _ => HttpStatusCode.Conflict,
            _ => HttpStatusCode.InternalServerError
        };

        var response = exception switch
        {
            ValidationException ex => new ErrorResponse("Validation failed", statusCode.ToString(), ex.Errors),
            _ => new ErrorResponse(exception.Message, statusCode.ToString())
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var json = JsonSerializer.Serialize(response);
        await context.Response.WriteAsync(json);
    }
}
