using Microsoft.AspNetCore.Http.HttpResults;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class DeleteTodoItemRequest : ITenantId
{
    public string? TenantId { get; set; }
    public Ulid? TodoItemId { get; set; }
}

public class DeleteTodoItemRequestValidator : Validator<DeleteTodoItemRequest>
{
    public DeleteTodoItemRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TodoItemId).NotNull();
    }
}

public class DeleteTodoItemEndpoint(IDynamoDbStore ddbStore, Mapper mapper) : Endpoint<DeleteTodoItemRequest, Results<NoContent, NotFound<ApiErrorResponse>>>
{
    public override void Configure()
    {
        Delete("tenant/{TenantId}/todo/{TodoItemId}");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Delete TodoItem";
            s.Description = "Delete a TodoItem";
        });
        Validator<DeleteTodoItemRequestValidator>();
    }

    public override async Task<Results<NoContent, NotFound<ApiErrorResponse>>> ExecuteAsync(DeleteTodoItemRequest request, CancellationToken ct)
    {
        var args = mapper.DeleteTodoItemRequestToArgs(request);
        var result = await ddbStore.DeleteTodoItemAsync(args, ct);

        if (!result)
        {
            return TypedResults.NotFound(ApiErrorResponse.NotFound());
        }

        return TypedResults.NoContent();
    }
}
