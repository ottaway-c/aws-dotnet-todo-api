namespace Todo.Client;

public class CreateTodoItemRequest
{
    public required string Title { get; init; }
    public required string Notes { get; init; }
    public required Ulid TenantId { get; init; }
    public Ulid? IdempotencyToken { get; set; }
}

public class CreateTodoItemResponse
{
    public required TodoItemDto TodoItem { get; init; }
}

public class GetTodoItemResponse
{
    public required TodoItemDto TodoItem { get; init; }
}

public class ListTodoItemsResponse
{
    public required List<TodoItemDto> TodoItems { get; set; }
    public string? PaginationToken { get; set; }
}

public class UpdateTodoItemRequest
{
    public required string Title { get; init; }
    public required string Notes { get; init; }
    public required bool IsCompleted { get; init; }
}

public class UpdateTodoItemResponse
{
    public required TodoItemDto TodoItem { get; init; }
}

public class TodoItemDto
{
    public required Ulid TodoItemId { get; init; }
    public required string Title { get; init; }
    public required string Notes { get; init; }
    public required Ulid TenantId { get; init; }
    public required Ulid IdempotencyToken { get; init; }
    public required bool IsCompleted { get; init; }
    public required DateTime CreatedDate { get; init; }
    public required DateTime UpdatedDate { get; init; }
}

