using EfficientDynamoDb.Attributes;

namespace Todo.Core.Entities;

public class TodoItemEntity : BaseEntity
{
    public static string Pk(string tenantId, Ulid todoItemId) => $"TENANT#{tenantId}#TODOITEM#{todoItemId}";
    public static string Sk(Ulid todoItemId) => $"TODOITEM#{todoItemId}";
    public static string Gsi1Pk(string tenantId) => $"TENANT#{tenantId}";
    public static string Gsi1Sk(Ulid todoItemId) =>  $"TODOITEM#{todoItemId}";
    
    [DynamoDbProperty(nameof(GSI1PK))]
    public required string GSI1PK { get; init; }
    
    [DynamoDbProperty(nameof(GSI1SK))]
    public required string GSI1SK { get; init; }
    
    [DynamoDbProperty(nameof(TenantId))]
    public required string TenantId { get; init; }
    
    [DynamoDbProperty(nameof(TodoItemId), typeof(UlidConverter))]
    public required Ulid TodoItemId { get; init; }
    
    [DynamoDbProperty(nameof(Title))]
    public required string Title { get; init; }
    
    [DynamoDbProperty(nameof(Notes))]
    public required string Notes { get; init; }
    
    [DynamoDbProperty(nameof(IsCompleted))]
    public required bool IsCompleted { get; init; }
    
    [DynamoDbProperty(nameof(IdempotencyToken), typeof(UlidConverter))]
    public required Ulid IdempotencyToken { get; init; }

    public static TodoItemEntity Create(CreateTodoItemArgs args)
    {
        var now = DateTime.UtcNow;
        var todoItemId = Ulid.NewUlid();

        var pk = Pk(args.TenantId, todoItemId);
        var sk = Sk(todoItemId);

        var gs1Pk = Gsi1Pk(args.TenantId);
        var gs1Sk = Gsi1Sk(todoItemId);
        
        var entity = new TodoItemEntity
        {
            PK = pk,
            SK = sk,
            GSI1PK = gs1Pk,
            GSI1SK = gs1Sk,
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