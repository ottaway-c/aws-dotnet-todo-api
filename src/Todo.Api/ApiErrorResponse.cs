namespace Todo.Api;

public class ApiError
{
    public required string Key { get; set; }
    public required List<string> Errors { get; set; } = [];
}

public class ApiErrorResponse
{
    public required string ErrorCode { get; set; }
    public required string ErrorMessage { get; set; }
    public required int StatusCode { get; set; }
    public required List<ApiError> Errors { get; set; }

    public static ApiErrorResponse NotFound() =>
        new()
        {
            StatusCode = 404,
            ErrorCode = "NotFoundError",
            ErrorMessage = "Not found",
            Errors = []
        };
    
    public static ApiErrorResponse ValidationError() =>
        new()
        {
            ErrorCode = "ValidationError",
            ErrorMessage = "Validation error",
            StatusCode = 400,
            Errors = []
        };

    public static ApiErrorResponse InternalServerError() =>
        new()
        {
            ErrorCode = "InternalServerError",
            ErrorMessage = "Internal server error",
            StatusCode = 500,
            Errors = []
        };
}