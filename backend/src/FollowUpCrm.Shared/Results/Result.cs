namespace FollowUpCrm.Shared.Results;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public int ErrorCode { get; }

    protected Result()
    {
        IsSuccess = true;
        Error = null;
        ErrorCode = 0;
    }

    protected Result(string error, int errorCode)
    {
        IsSuccess = false;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new();
    public static Result Failure(string error, int errorCode = 400) => new(error, errorCode);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value) : base()
    {
        Value = value;
    }

    private Result(string error, int errorCode) : base(error, errorCode)
    {
        Value = default;
    }

    public static Result<T> Success(T value) => new(value);
    public new static Result<T> Failure(string error, int errorCode = 400) => new(error, errorCode);

    public static implicit operator Result<T>(T value) => Success(value);
}