using System.Net;
using System.Text.Json;
using AINews.Application.Common.Exceptions;
using ValidationException = AINews.Application.Common.Exceptions.ValidationException;

namespace AINews.API.Middleware;

/// <summary>
/// Translates exceptions thrown by Application-layer handlers into
/// consistent HTTP problem responses, so controllers stay free of try/catch.
/// </summary>
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
        catch (ValidationException ex)
        {
            await WriteProblem(context, HttpStatusCode.BadRequest, "Validation Failed", ex.Message, new { errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, HttpStatusCode.NotFound, "Not Found", ex.Message);
        }
        catch (AuthenticationFailedException ex)
        {
            await WriteProblem(context, HttpStatusCode.Unauthorized, "Authentication Failed", ex.Message);
        }
        catch (ForbiddenAccessException)
        {
            await WriteProblem(context, HttpStatusCode.Forbidden, "Forbidden", "You do not have permission to perform this action.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);
            await WriteProblem(context, HttpStatusCode.InternalServerError, "Server Error", "An unexpected error occurred.");
        }
    }

    private static Task WriteProblem(HttpContext context, HttpStatusCode statusCode, string title, string detail, object? extra = null)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var body = new Dictionary<string, object?>
        {
            ["status"] = (int)statusCode,
            ["title"] = title,
            ["detail"] = detail
        };

        if (extra is not null)
        {
            foreach (var prop in extra.GetType().GetProperties())
            {
                body[prop.Name] = prop.GetValue(extra);
            }
        }

        return context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
