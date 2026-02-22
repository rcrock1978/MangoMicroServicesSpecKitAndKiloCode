namespace Mango.Common;

/// <summary>
/// Standard API response wrapper for all API responses
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse()
    {
    }

    public ApiResponse(T data, string? message = null)
    {
        Success = true;
        Data = data;
        Message = message;
    }

    public ApiResponse(string message, List<string>? errors = null)
    {
        Success = false;
        Message = message;
        Errors = errors;
    }

    public static ApiResponse<T> SuccessResult(T data, string? message = null) => new(data, message);
    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null) => new(message, errors);
}

/// <summary>
/// Paged response for list endpoints
/// </summary>
public class PagedResponse<T> : ApiResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;

    public PagedResponse()
    {
    }

    public PagedResponse(T items, int pageNumber, int pageSize, int totalRecords)
    {
        Data = items;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
        Success = true;
    }
}

/// <summary>
/// Standard error response
/// </summary>
public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<string>? Details { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
