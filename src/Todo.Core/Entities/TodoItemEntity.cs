using EfficientDynamoDb.Attributes;

namespace Todo.Core.Entities;

public class TodoItemEntity : BaseEntity
{
    [DynamoDbProperty(nameof(TenantId))]
    public required string TenantId { get; set; }
    
    [DynamoDbProperty(nameof(TodoItemId), typeof(UlidConverter))]
    public required Ulid TodoItemId { get; init; }
    
    [DynamoDbProperty(nameof(Title))]
    public required string Title { get; init; }
    
    [DynamoDbProperty(nameof(Notes))]
    public required string Notes { get; init; }
    
    [DynamoDbProperty(nameof(IsCompleted))]
    public required bool IsCompleted { get; init; }
    
    [DynamoDbProperty(nameof(IdempotencyToken), typeof(UlidConverter))]
    public required Ulid IdempotencyToken { get; set; }

    public static TodoItemEntity Create(CreateTodoItemArgs args)
    {
        var now = DateTime.UtcNow;
        var todoItemId = Ulid.NewUlid();
        
        var pk = $"TENANT#{args.TenantId}";
        var sk = $"TODOITEM#{todoItemId}";
        
        var entity = new TodoItemEntity
        {
            PK = pk,
            SK = sk,
            TodoItemId = todoItemId,
            Title = args.Title,
            Notes = args.Notes,
            IsCompleted = false,
            TenantId = args.TenantId,
            IdempotencyToken = args.IdempotencyToken,
            CreatedDate = now,
            UpdatedDate = now,
            Entity = "TodoItem"
        };

        return entity;
    }
}