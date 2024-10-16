using System.ComponentModel;
using Microsoft.AspNetCore.Http.HttpResults;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class DeleteTodoItemRequest
{
    [DefaultValue(Constants.DefaultTodoItemId)]
    public Ulid? TodoItemId { get; set; }
    
    [DefaultValue(Constants.DefaultTenantId)]
    public Ulid? TenantId { get; set; }
}

public class DeleteTodoItemRequestValidator : Validator<DeleteTodoItemRequest>
{
    public DeleteTodoItemRequestValidator()
    {
        RuleFor(x => x.TodoItemId).NotNull();
        RuleFor(x => x.TenantId).NotNull();
    }
}

public class DeleteTodoItemEndpoint(IDynamoDbStore ddbStore, Mapper mapper) : Endpoint<DeleteTodoItemRequest, NoContent>
{
    public override void Configure()
    {
        Delete("/api/{TenantId}/todo/{TodoItemId}");
        Description(x => x.Produces(404));
        Description(x => x.ProducesProblemFE<InternalErrorResponse>(500));
        Summary(s =>
        {
            s.Summary = "Delete TodoItem";
            s.Description = "Delete a TodoItem";
            s.RequestParam(r => r.TodoItemId!, "TodoItem Id");
            s.RequestParam(r => r.TenantId!, "Tenant Id");
        });
        Validator<DeleteTodoItemRequestValidator>();
    }
    
    public override async Task HandleAsync(DeleteTodoItemRequest request, CancellationToken ct)
    {
        var args = mapper.DeleteTodoItemRequestToArgs(request);
        var result = await ddbStore.DeleteTodoItemAsync(args, ct);

        if (!result)
        {
            await SendNotFoundAsync(ct);
        }
    }
}