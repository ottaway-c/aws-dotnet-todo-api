using EfficientDynamoDb.Attributes;

namespace Todo.Core.Entities;

public class IdempotencyEntity : BaseEntity
{
    public static string Pk(string tenantId, Ulid idempotencyToken) => $"TENANT#{tenantId}#IDEMPOTENCY#{idempotencyToken}#";
    public static string Sk(Ulid idempotencyToken) =>  $"IDEMPOTENCY#{idempotencyToken}";
    
    [DynamoDbProperty(nameof(TodoItemId), typeof(UlidConverter))]
    public required Ulid TodoItemId { get; init; }
    
    [DynamoDbProperty(nameof(TenantId))]
    public required string TenantId { get; init; }
    
    [DynamoDbProperty(nameof(IdempotencyToken), typeof(UlidConverter))]
    public required Ulid IdempotencyToken { get; init; }
    
    public static IdempotencyEntity Create(CreateTodoItemArgs args, TodoItemEntity todoItemEntity)
    {
        var pk = Pk(args.TenantId, args.IdempotencyToken);
        var sk = Sk(args.IdempotencyToken);

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