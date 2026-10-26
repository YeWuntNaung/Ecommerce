using Ecommerce.Domain.Primitives;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.Api.Common;

public static class ResultExtensions
{
    public static IResult ToProblem(this Error error) => error.Type switch
    {
        ErrorType.Validation => Results.ValidationProblem(
            error.ValidationErrors ?? new Dictionary<string, string[]>(),
            detail: error.Message,
            statusCode: StatusCodes.Status400BadRequest),
        ErrorType.NotFound => Results.Problem(
            detail: error.Message,
            statusCode: StatusCodes.Status404NotFound,
            title: "Not found"),
        ErrorType.Conflict => Results.Problem(
            detail: error.Message,
            statusCode: StatusCodes.Status409Conflict,
            title: error.Code),
        ErrorType.Forbidden => Results.Problem(
            detail: error.Message,
            statusCode: StatusCodes.Status403Forbidden,
            title: "Forbidden"),
        ErrorType.Unauthorized => Results.Problem(
            detail: error.Message,
            statusCode: StatusCodes.Status401Unauthorized,
            title: "Unauthorized"),
        _ => Results.Problem(
            detail: error.Message,
            statusCode: StatusCodes.Status500InternalServerError,
            title: "Failure"),
    };

    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? Results.NoContent() : result.Error!.ToProblem();

    public static IResult ToHttpResult<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : result.Error!.ToProblem();
}
