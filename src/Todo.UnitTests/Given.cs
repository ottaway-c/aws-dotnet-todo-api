using System.Collections;
using Todo.Api.Endpoints;
using Todo.Core;

namespace Todo.UnitTests;

public static class Given
{
    public static CreateTodoItemRequest CreateTodoItemRequest()
    {
        var request = new CreateTodoItemRequest
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            TenantId = Ulid.NewUlid().ToString(),
            IdempotencyToken = Ulid.NewUlid(),
        };

        return request;
    }

    public static UpdateTodoItemRequest UpdateTodoItemRequest()
    {
        var request = new UpdateTodoItemRequest
        {
            TodoItemId = Ulid.NewUlid(),
            TenantId = Ulid.NewUlid().ToString(),
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            IsCompleted = true,
        };

        return request;
    }

    public static DeleteTodoItemRequest DeleteTodoItemRequest()
    {
        var request = new DeleteTodoItemRequest { TodoItemId = Ulid.NewUlid(), TenantId = Ulid.NewUlid().ToString() };

        return request;
    }

    public static GetTodoItemRequest GetTodoItemRequest()
    {
        var request = new GetTodoItemRequest { TodoItemId = Ulid.NewUlid(), TenantId = Ulid.NewUlid().ToString() };

        return request;
    }

    public static ListTodoItemsRequest ListTodoItemsRequest()
    {
        var request = new ListTodoItemsRequest
        {
            TenantId = Ulid.NewUlid().ToString(),
            Limit = 20,
            PaginationToken = Ulid.NewUlid().ToString(),
            IsCompleted = true,
        };

        return request;
    }

    public static CreateTodoItemArgs CreateTodoItemArgs()
    {
        var args = new CreateTodoItemArgs
        {
            Title = Ulid.NewUlid().ToString(),
            Notes = Ulid.NewUlid().ToString(),
            TenantId = Ulid.NewUlid().ToString(),
            IdempotencyToken = Ulid.NewUlid(),
        };

        return args;
    }

    public class TitleTestData : IEnumerable<object?[]>
    {
        public IEnumerator<object?[]> GetEnumerator()
        {
            yield return new object?[] { null };
            yield return new object[] { "" };
            yield return new object[] { " " };
            yield return new object[] { new string('*', 2) }; // Note: Minimum length is 3
            yield return new object[] { new string('*', 101) }; // Note: Maximum length is 100
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class NotesTestData : IEnumerable<object?[]>
    {
        public IEnumerator<object?[]> GetEnumerator()
        {
            yield return new object?[] { null };
            yield return new object[] { "" };
            yield return new object[] { " " };
            yield return new object[] { new string('*', 2) }; // Note: Minimum length is 3
            yield return new object[] { new string('*', 101) }; // Note: Maximum length is 100
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
