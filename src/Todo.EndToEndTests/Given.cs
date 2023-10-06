using Todo.Client;
using Todo.Core;

namespace Todo.EndToEndTests;

public static class Given
{
    public static CreateTodoItemRequest CreateTodoItemRequest()
    {
        var request = new CreateTodoItemRequest
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            TenantId = Ulid.NewUlid()
        };

        return request;
    }

    public static UpdateTodoItemRequest UpdateTodoItemRequest()
    {
        var request = new UpdateTodoItemRequest
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            IsCompleted = true
        };

        return request;
    }
    
    public static CreateTodoItemArgs CreateTodoItemArgs(Ulid? tenantId = null)
    {
        var args = new CreateTodoItemArgs
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            TenantId = tenantId ?? Ulid.NewUlid(),
            IdempotencyToken = Ulid.NewUlid()
        };

        return args;
    }
}