namespace Inventory.Api.Responses;

public sealed record ApiResponse<T>(bool Success, T? Data, string? Error)
{
    public static ApiResponse<T> Ok(T data)
    {
        return new ApiResponse<T>(true, data, null);
    }

    public static ApiResponse<T> Fail(string error)
    {
        return new ApiResponse<T>(false, default, error);
    }
}
