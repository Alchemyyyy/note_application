namespace NotesApp.Api.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IEnumerable<string>? Errors { get; init; }
    public string? TraceId { get; init; }
}

public static class ApiResponse
{
    public static ApiResponse<T> Success<T>(T data, string message = "Success", string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            TraceId = traceId
        };
    }

    public static ApiResponse<object?> Failure(string message, IEnumerable<string>? errors = null, string? traceId = null)
    {
        return new ApiResponse<object?>
        {
            Success = false,
            Message = message,
            Errors = errors,
            TraceId = traceId
        };
    }
}
