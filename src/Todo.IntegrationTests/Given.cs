using Todo.Api.Endpoints;
using Todo.Core;

namespace Todo.IntegrationTests;

public static class Given
{
    public static CreateTodoItemRequest CreateTodoItemRequest(string tenantId, Ulid? idempotencyToken = null)
    {
        var request = new CreateTodoItemRequest
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            TenantId = tenantId,
            IdempotencyToken = idempotencyToken,
        };

        return request;
    }

    public static UpdateTodoItemRequest UpdateTodoItemRequest(string tenantId, Ulid todoItemId)
    {
        var request = new UpdateTodoItemRequest
        {
            TenantId = tenantId,
            TodoItemId = todoItemId,
            IsCompleted = true,
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
        };

        return request;
    }

    public static GetTodoItemRequest GetTodoItemRequest(string tenantId, Ulid todoItemId)
    {
        var request = new GetTodoItemRequest { TodoItemId = todoItemId, TenantId = tenantId };

        return request;
    }

    public static ListTodoItemsRequest ListTodoItemsRequest(string tenantId, int? limit = null, string? paginationToken = null, bool? isCompleted = false)
    {
        var request = new ListTodoItemsRequest
        {
            TenantId = tenantId,
            Limit = limit,
            PaginationToken = paginationToken,
            IsCompleted = isCompleted,
        };

        return request;
    }

    public static CreateTodoItemArgs CreateTodoItemArgs(string? tenantId = null)
    {
        var args = new CreateTodoItemArgs
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            TenantId = tenantId ?? TenantId(),
            IdempotencyToken = Ulid.NewUlid(),
        };

        return args;
    }

    public static DeleteTodoItemRequest DeleteTodoItemRequest(string tenantId, Ulid todoItemId)
    {
        var request = new DeleteTodoItemRequest { TodoItemId = todoItemId, TenantId = tenantId };

        return request;
    }

    public static string TenantId()
    {
        var tenantId = Ulid.NewUlid().ToString().ToLower();

        return tenantId;
    }
}
