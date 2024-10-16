using Microsoft.AspNetCore.Http.HttpResults;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class GetTodoItemRequest : ITenantId
{
    public string? TenantId { get; set; }
    public Ulid? TodoItemId { get; set; }
}

public class GetTodoItemResponse
{
    public required TodoItemDto TodoItem { get; init; }
}

public class GetTodoItemRequestValidator : Validator<GetTodoItemRequest>
{
    public GetTodoItemRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TodoItemId).NotNull();
    }
}

public class GetTodoItemEndpoint(IDynamoDbStore ddbStore, Mapper mapper)
    : Endpoint<GetTodoItemRequest, Results<Ok<GetTodoItemResponse>, NotFound<ApiErrorResponse>>>
{
    public override void Configure()
    {
        Get("tenant/{TenantId}/todo/{TodoItemId}");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Get TodoItem";
            s.Description = "Get a TodoItem";
        });
        Validator<GetTodoItemRequestValidator>();
    }

    public override async Task<Results<Ok<GetTodoItemResponse>, NotFound<ApiErrorResponse>>> ExecuteAsync(GetTodoItemRequest request, CancellationToken ct)
    {
        var entity = await ddbStore.GetTodoItemAsync(request.TenantId!, request.TodoItemId!.Value, ct);
        
        if (entity == null)
        {
            return TypedResults.NotFound(ApiErrorResponse.NotFound());
        }
        
        var todoItemDto = mapper.TodoItemEntityToDto(entity);

        var response = new GetTodoItemResponse
        {
            TodoItem = todoItemDto
        };

        return TypedResults.Ok(response);
    }
}