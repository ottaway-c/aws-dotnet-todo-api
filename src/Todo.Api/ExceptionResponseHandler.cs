using System.Text.Json;

namespace Todo.Api;

public class ExceptionResponseHandler : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case JsonException jsonEx:
            {
                var errorResponse = ApiErrorResponse.ValidationError();
                context.Response.StatusCode = errorResponse.StatusCode;
                errorResponse.Errors.Add(new ApiError { Key = "JsonException", Errors = [jsonEx.Message] });
                await context.Response.WriteAsJsonAsync(errorResponse, cancellationToken: cancellationToken);
                return true;
            }
            case ValidationFailureException validationEx:
            {
                var errorResponse = ApiErrorResponse.ValidationError();
                context.Response.StatusCode = errorResponse.StatusCode;
                if (validationEx.Failures is not null)
                {
                    foreach (var grouping in validationEx.Failures.GroupBy(x => x.PropertyName))
                    {
                        errorResponse.Errors.Add(new ApiError { Key = grouping.Key, Errors = grouping.Select(x => x.ErrorMessage).ToList() });
                    }
                }
                await context.Response.WriteAsJsonAsync(errorResponse, cancellationToken: cancellationToken);
                return true;
            }
            default:
            {
                var apiErrorResponse = ApiErrorResponse.InternalServerError();
                context.Response.StatusCode = apiErrorResponse.StatusCode;
                await context.Response.WriteAsJsonAsync(apiErrorResponse, cancellationToken: cancellationToken);
                return true;
            }
        }
    }
}