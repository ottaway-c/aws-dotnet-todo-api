namespace Todo.Core;

public class TodoItemDto
{
    public required Ulid TodoItemId { get; set; }

    public required string Title { get; set; }

    public required string Notes { get; set; }

    public required bool IsCompleted { get; set; }

    public required string TenantId { get; set; }

    public required Ulid IdempotencyToken { get; set; }

    public required DateTime CreatedDate { get; set; }

    public required DateTime UpdatedDate { get; set; }
}
