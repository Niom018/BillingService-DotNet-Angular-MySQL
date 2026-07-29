using BillingService.Application.Exceptions;

namespace BillingService.Api.Middleware;

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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException nf =>
                (StatusCodes.Status404NotFound, "Not Found", nf.Message),

            FluentValidation.ValidationException ve =>
                (StatusCodes.Status400BadRequest, "Validation Failed",
                    string.Join(" ", ve.Errors.Select(e => e.ErrorMessage))),

            InvalidOperationException ioe =>
                (StatusCodes.Status400BadRequest, "Invalid Operation", ioe.Message),

            ArgumentException ae =>
                (StatusCodes.Status400BadRequest, "Invalid Argument", ae.Message),

            _ => (StatusCodes.Status500InternalServerError, "Server Error", "An unexpected error occurred.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception");
        else
            _logger.LogWarning("Handled exception ({Title}): {Message}", title, detail);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problem = new
        {
            title,
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(problem);
    }
}
