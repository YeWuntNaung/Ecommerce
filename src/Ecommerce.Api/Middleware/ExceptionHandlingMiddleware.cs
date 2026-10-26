using Ecommerce.Domain.Primitives;

namespace Ecommerce.Api.Middleware;

/// <summary>Maps domain invariant violations to 400 ProblemDetails and everything else to a 500 without leaking internals.</summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException exception)
        {
            await WriteProblemAsync(
                context,
                StatusCodes.Status400BadRequest,
                "Domain rule violated",
                exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

            await WriteProblemAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected error",
                "An unexpected error occurred. The failure has been logged.");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.Clear();
        await Results.Problem(statusCode: statusCode, title: title, detail: detail).ExecuteAsync(context);
    }
}
