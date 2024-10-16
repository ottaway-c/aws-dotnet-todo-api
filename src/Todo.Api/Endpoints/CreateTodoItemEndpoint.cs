using Microsoft.AspNetCore.Http.HttpResults;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class CreateTodoItemRequest : ITenantId
{
    public string? TenantId { get; set; }
    public string? Title { get; set; }
    public string? Notes { get; set; }
    public Ulid? IdempotencyToken { get; set; }
}

public class CreateTodoItemResponse
{
    public required TodoItemDto TodoItem { get; init; }
}

public class CreateTodoItemRequestValidator : Validator<CreateTodoItemRequest>
{
    public CreateTodoItemRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Notes).NotEmpty().MinimumLength(3).MaximumLength(100);
    }
}

public class CreateTodoItemEndpoint(IDynamoDbStore ddb, Mapper mapper)
    : Endpoint<CreateTodoItemRequest, Ok<CreateTodoItemResponse>>
{
    public override void Configure()
    {
        Post("tenant/{TenantId}/todo");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Create TodoItem";
            s.Description = "Create a new TodoItem";
        });
        Validator<CreateTodoItemRequestValidator>();
    }

    public override async Task<Ok<CreateTodoItemResponse>> ExecuteAsync(CreateTodoItemRequest request, CancellationToken ct)
    {
        // Note: Assign an idempotency token if the request does not contain one
        request.IdempotencyToken ??= Ulid.NewUlid();

        var args = mapper.CreateTodoItemRequestToArgs(request);
        var entity = await ddb.CreateTodoItemAsync(args, ct);

        var response = new CreateTodoItemResponse
        {
            TodoItem = mapper.TodoItemEntityToDto(entity)
        };

        return TypedResults.Ok(response);
    }
}