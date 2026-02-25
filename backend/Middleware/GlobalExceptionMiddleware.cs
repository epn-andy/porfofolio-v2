using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using PortfolioAPI.Exceptions;

namespace PortfolioAPI.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        ExceptionProblemDetail problem;

        switch (ex)
        {
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                problem = new ExceptionProblemDetail
                {
                    Id = Guid.NewGuid(),
                    Title = ex.Message,
                    Status = (int)statusCode,
                    Type = nameof(UnauthorizedAccessException),
                    Detail = ex.InnerException?.Message,
                };
                break;

            case BadRequestException badReq:
                statusCode = HttpStatusCode.BadRequest;
                problem = new ExceptionProblemDetail
                {
                    Id = Guid.NewGuid(),
                    Title = badReq.Message,
                    Status = (int)statusCode,
                    Type = nameof(BadRequestException),
                    Detail = badReq.InnerException?.Message,
                    Errors = badReq.ValidationError,
                };
                break;

            case NotFoundException notFound:
                statusCode = HttpStatusCode.NotFound;
                problem = new ExceptionProblemDetail
                {
                    Id = Guid.NewGuid(),
                    Title = notFound.Message,
                    Status = (int)statusCode,
                    Type = nameof(NotFoundException),
                    Detail = notFound.InnerException?.Message,
                };
                break;

            case ConflictException conflict:
                statusCode = HttpStatusCode.Conflict;
                problem = new ExceptionProblemDetail
                {
                    Id = Guid.NewGuid(),
                    Title = conflict.Message,
                    Status = (int)statusCode,
                    Type = nameof(ConflictException),
                    Detail = conflict.InnerException?.Message,
                };
                break;

            case NotAcceptableException notAcceptable:
                statusCode = HttpStatusCode.NotAcceptable;
                problem = new ExceptionProblemDetail
                {
                    Id = Guid.NewGuid(),
                    Title = notAcceptable.Message,
                    Status = (int)statusCode,
                    Type = nameof(NotAcceptableException),
                    Detail = notAcceptable.InnerException?.Message,
                };
                break;

            case ForbiddenException forbidden:
                statusCode = HttpStatusCode.Forbidden;
                problem = new ExceptionProblemDetail
                {
                    Id = Guid.NewGuid(),
                    Title = forbidden.Message,
                    Status = (int)statusCode,
                    Type = nameof(ForbiddenException),
                    Detail = forbidden.InnerException?.Message,
                };
                break;

            default:
                problem = new ExceptionProblemDetail
                {
                    Id = Guid.NewGuid(),
                    Title = ex.Message,
                    Status = (int)statusCode,
                    Type = nameof(HttpStatusCode.InternalServerError),
                    Detail = ex.StackTrace,
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
