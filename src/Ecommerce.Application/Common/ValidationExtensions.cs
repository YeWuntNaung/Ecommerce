using Ecommerce.Domain.Primitives;
using FluentValidation;

namespace Ecommerce.Application.Common;

public static class ValidationExtensions
{
    public static Error ToError(this FluentValidation.Results.ValidationResult result) =>
        Error.Validation(result.Errors
            .GroupBy(failure => ToCamelCase(failure.PropertyName))
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray()));

    private static string ToCamelCase(string value) =>
        string.IsNullOrEmpty(value) ? value : char.ToLowerInvariant(value[0]) + value[1..];
}
