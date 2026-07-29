namespace MyApp.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public int Status { get; set; }     
    public int Code { get; set; }     
    public string? Message { get; set; }
    public string? ActionType { get; set; }
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> Ok(T data, string? message = null, string? actionType = null,int code=-1)
        => new()
        {
            Success = true,
            Status = 200,              
            Message = message ?? "Successful",
            Data = data,
           Code=code

        };

    public static ApiResponse<T> Fail(string message, params string[] errors)
        => new()
        {
            Success = false,
            Status = 400,              
            Message = message,
            Errors = errors.ToList()
        };
}
public class PagedResponse<T> : ApiResponse<IEnumerable<T>>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public static PagedResponse<T> Ok(
        IEnumerable<T> data,
        int page,
        int pageSize,
        long totalCount,
        string? message = null)
    {
        return new PagedResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
