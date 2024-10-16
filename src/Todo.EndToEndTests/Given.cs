using Todo.Client.Models;
using Todo.Core;

namespace Todo.EndToEndTests;

public static class Given
{
    public static CreateTodoItemRequest CreateTodoItemRequest()
    {
        var request = new CreateTodoItemRequest
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString()
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
    
    public static CreateTodoItemArgs CreateTodoItemArgs(string? tenantId = null)
    {
        var args = new CreateTodoItemArgs
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            TenantId = tenantId ?? TenantId(),
            IdempotencyToken = Ulid.NewUlid()
        };

        return args;
    }
    
    public static string TenantId()
    {
        var tenantId = Ulid.NewUlid().ToString().ToLower();

        return tenantId;
    }
}