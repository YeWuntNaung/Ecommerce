namespace Ecommerce.Domain.Primitives;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized,
    Failure,
}

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type,
    IReadOnlyDictionary<string, string[]>? ValidationErrors = null)
{
    public static Error Validation(string field, string message) =>
        new("validation_error", message, ErrorType.Validation,
            new Dictionary<string, string[]> { [field] = [message] });

    public static Error Validation(IReadOnlyDictionary<string, string[]> errors) =>
        new("validation_error", "One or more validation errors occurred.", ErrorType.Validation, errors);

    public static Error NotFound(string message) => new("not_found", message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string message = "You are not allowed to perform this action.") =>
        new("forbidden", message, ErrorType.Forbidden);

    public static Error Unauthorized() => new("unauthorized", "Authentication is required.", ErrorType.Unauthorized);

    public static Error Failure(string message) => new("failure", message, ErrorType.Failure);
}
