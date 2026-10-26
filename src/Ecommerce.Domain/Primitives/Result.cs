namespace Ecommerce.Domain.Primitives;

/// <summary>Explicit success/failure of a use case. Expected failures use this; exceptions mean bugs.</summary>
public sealed class Result
{
    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public static Result Ok() => new(true, null);

    public static Result Fail(Error error) => new(false, error);

    public static Result<T> Ok<T>(T value) => Result<T>.Ok(value);

    public static Result<T> Fail<T>(Error error) => Result<T>.Fail(error);
}

public sealed class Result<T>
{
    private readonly T? _value;

    private Result(bool isSuccess, T? value, Error? error)
    {
        IsSuccess = isSuccess;
        _value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public Error? Error { get; }

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot read the value of a failed result.");

    public static Result<T> Ok(T value) => new(true, value, null);

    public static Result<T> Fail(Error error) => new(false, default, error);
}
