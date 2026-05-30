using System.Text.Json.Serialization;

namespace FollowUpCrm.Shared.Results;

public sealed class ApiResponse<T>
{
    private ApiResponse(bool success, string message, T? data, IReadOnlyCollection<string>? errors)
    {
        Success = success;
        Message = message;
        Data = data;
        Errors = errors;
    }

    public bool Success { get; }

    public string Message { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T? Data { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyCollection<string>? Errors { get; }

    public static ApiResponse<T> SuccessResponse(
        T data,
        string message = "Operation completed successfully")
        => new(true, message, data, null);

    public static ApiResponse<T> FailureResponse(
        string message,
        IEnumerable<string>? errors = null)
        => new(false, message, default, errors?.ToArray() ?? Array.Empty<string>());
}
