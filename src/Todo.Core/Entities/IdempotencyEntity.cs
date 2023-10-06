using EfficientDynamoDb.Attributes;

namespace Todo.Core.Entities;

public class IdempotencyEntity : BaseEntity
{
    [DynamoDbProperty(nameof(TodoItemId), typeof(UlidConverter))]
    public required Ulid TodoItemId { get; set; }
    
    [DynamoDbProperty(nameof(TenantId), typeof(UlidConverter))]
    public required Ulid TenantId { get; set; }
    
    [DynamoDbProperty(nameof(IdempotencyToken), typeof(UlidConverter))]
    public required Ulid IdempotencyToken { get; set; }
    
    public static IdempotencyEntity Create(CreateTodoItemArgs args, TodoItemEntity todoItemEntity)
    {
        var pk = $"IDEMPOTENCY#{args.IdempotencyToken}#TENANT#{args.TenantId}";
        var sk = $"IDEMPOTENCY#{args.IdempotencyToken}";

        var entity = new IdempotencyEntity
        {
            PK = pk,
            SK = sk,
            TodoItemId = todoItemEntity.TodoItemId,
            TenantId = args.TenantId,
            IdempotencyToken = args.IdempotencyToken,
            CreatedDate = todoItemEntity.CreatedDate,
            UpdatedDate = todoItemEntity.UpdatedDate,
            Entity = "Idempotency"
        };

        return entity;
    }
}