using Microsoft.AspNetCore.Http.HttpResults;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class UpdateTodoItemRequest : ITenantId
{
    public string? TenantId { get; set; }
    public Ulid? TodoItemId { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public bool? IsCompleted { get; set; }
}

public class UpdateTodoItemResponse
{
    public required TodoItemDto TodoItem { get; init; }
}

public class UpdateTodoItemRequestValidator : Validator<UpdateTodoItemRequest>
{
    public UpdateTodoItemRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.TodoItemId).NotNull();
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Notes).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.IsCompleted).NotNull();
    }
}

public class UpdateTodoItemEndpoint(IDynamoDbStore ddbStore, Mapper mapper)
    : Endpoint<UpdateTodoItemRequest, Results<Ok<UpdateTodoItemResponse>, NotFound<ApiErrorResponse>>>
{
    public override void Configure()
    {
        Put("tenant/{TenantId}/todo/{TodoItemId}");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Update TodoItem";
            s.Description = "Update a TodoItem";
            
        });
        Validator<UpdateTodoItemRequestValidator>();
    }

    public override async Task<Results<Ok<UpdateTodoItemResponse>, NotFound<ApiErrorResponse>>> ExecuteAsync(UpdateTodoItemRequest request, CancellationToken ct)
    {
        var args = mapper.UpdateTodoItemRequestToArgs(request);
        var entity = await ddbStore.UpdateTodoItemAsync(args, ct);
    
        if (entity == null)
        {
            return TypedResults.NotFound(ApiErrorResponse.NotFound());
        }
    
        var todoItemDto = mapper.TodoItemEntityToDto(entity);

        var response = new UpdateTodoItemResponse
        {
            TodoItem = todoItemDto
        };

        return TypedResults.Ok(response);
    }
}