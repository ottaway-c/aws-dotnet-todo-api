using EfficientDynamoDb;
using EfficientDynamoDb.Converters;
using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Exceptions;
using EfficientDynamoDb.Operations.Shared;
using Todo.Core.Entities;

namespace Todo.Core;

public class CreateTodoItemArgs
{
    public required string TenantId { get; init; }
    public required string Title { get; init; }
    public required string Notes { get; init; }
    public required Ulid IdempotencyToken { get; init; }
}

public class UpdateTodoItemArgs
{
    public required Ulid TodoItemId { get; init; }
    public required string TenantId { get; init; }
    public required string Title { get; init; }
    public required string Notes { get; init; }
    public required bool IsCompleted { get; init; }
}

public class DeleteTodoItemArgs
{
    public required Ulid TodoItemId { get; init; }
    public required string TenantId { get; init; }
}

public interface IDynamoDbStore
{
    Task<TodoItemEntity> CreateTodoItemAsync(CreateTodoItemArgs args, CancellationToken ct);

    Task<TodoItemEntity?> UpdateTodoItemAsync(UpdateTodoItemArgs args, CancellationToken ct);

    Task<bool> DeleteTodoItemAsync(DeleteTodoItemArgs args, CancellationToken ct);

    Task<TodoItemEntity?> GetTodoItemAsync(string tenantId, Ulid todoItemId, CancellationToken ct);

    Task<PagedResult<TodoItemEntity>> ListTodoItemsAsync(string tenantId, int limit, bool? isComplete = null, string? paginationToken = null);
}

public class DynamoDbStore(IDynamoDbContext ddb) : IDynamoDbStore
{
    private const string Gsi1IndexName = "GSI1";
    
    public async Task<TodoItemEntity> CreateTodoItemAsync(CreateTodoItemArgs args, CancellationToken ct)
    {
        var entity = TodoItemEntity.Create(args);
        var idempotencyEntity = IdempotencyEntity.Create(args, entity);

        var entityFilterExpression = Condition<TodoItemEntity>.On(x => x.PK).NotExists();
        var idempotencyEntityFilterExpression = Condition<IdempotencyEntity>.On(x => x.PK).NotExists();

        try
        {
            await ddb.TransactWrite().WithItems(
                Transact.PutItem(entity).WithCondition(entityFilterExpression),
                Transact.PutItem(idempotencyEntity)
                    .WithCondition(idempotencyEntityFilterExpression)
                    .WithReturnValuesOnConditionCheckFailure(ReturnValuesOnConditionCheckFailure.AllOld)
            ).ExecuteAsync(ct);

            return entity;
        }
        catch (TransactionCanceledException ex)
        {
            if (ex.CancellationReasons.Any(x => x.Code == "ConditionalCheckFailed"))
            {
                // Note:
                // Conditional check failures are returned in the order they are supplied to Dynamo.
                // E.g in this case we need to look at the second item in the array, as we expect the idempotency check to have failed.
                // From there we can extract the PK/SK of the TodoItem entity associated with the idempotency token, and return it.
                var document = ex.CancellationReasons[1].Item;
                var duplicateEntity = ddb.ToObject<IdempotencyEntity>(document!);

                entity = await GetTodoItemAsync(duplicateEntity.TenantId, duplicateEntity.TodoItemId, ct);
                if (entity == null) throw new InvalidOperationException("Failed to find duplicate TodoItem entity");
                
                return entity;
            }

            throw;
        }
    }

    public async Task<TodoItemEntity?> UpdateTodoItemAsync(UpdateTodoItemArgs args, CancellationToken ct)
    {
        var pk = TodoItemEntity.Pk(args.TenantId, args.TodoItemId);
        var sk = TodoItemEntity.Sk(args.TodoItemId);
        var now = DateTime.UtcNow;
        
        var condition = Condition.ForEntity<TodoItemEntity>();
        var expression = Joiner.And(condition.On(x => x.PK).Exists(),
            condition.On(x => x.SK).Exists());

        try
        {
            var entity = await ddb.UpdateItem<TodoItemEntity>()
                .WithPrimaryKey(pk, sk)
                .WithCondition(expression)
                .On(x => x.Title).Assign(args.Title)
                .On(x => x.Notes).Assign(args.Notes)
                .On(x => x.IsCompleted).Assign(args.IsCompleted)
                .On(x => x.UpdatedDate).Assign(now)
                .WithReturnValues(ReturnValues.AllNew)
                .ToItemAsync(ct);
            
            return entity;
        }
        catch (ConditionalCheckFailedException)
        {
            // Note: Item does not exist, return null
            return null;
        }
    }

    public async Task<bool> DeleteTodoItemAsync(DeleteTodoItemArgs args, CancellationToken ct)
    {
        var pk = TodoItemEntity.Pk(args.TenantId, args.TodoItemId);
        var sk = TodoItemEntity.Sk(args.TodoItemId);
        
        var condition = Condition.ForEntity<TodoItemEntity>();
        var expression = Joiner.And(condition.On(x => x.PK).Exists(),
            condition.On(x => x.SK).Exists());
        
        try
        {
            await ddb.DeleteItem<TodoItemEntity>()
                .WithPrimaryKey(pk, sk)
                .WithCondition(expression)
                .ExecuteAsync(ct);

            return true;
        }
        catch (ConditionalCheckFailedException)
        {
            // Note: Item does not exist, return false
            return false;
        }
    }

    public async Task<TodoItemEntity?> GetTodoItemAsync(string tenantId, Ulid todoItemId, CancellationToken ct)
    {
        var pk = TodoItemEntity.Pk(tenantId, todoItemId);
        var sk = TodoItemEntity.Sk(todoItemId);
        
        var entity = await ddb.GetItemAsync<TodoItemEntity>(pk, sk, ct);

        return entity;
    }

    public async Task<PagedResult<TodoItemEntity>> ListTodoItemsAsync(string tenantId, int limit, bool? isComplete = null, string? paginationToken = null)
    {
        var gs1Pk = TodoItemEntity.Gsi1Pk(tenantId);
        
        var keyCondition = Condition.ForEntity<TodoItemEntity>();
        var keyExpression = Joiner.And(
            keyCondition.On(x => x.GSI1PK).EqualTo(gs1Pk),
            keyCondition.On(x => x.GSI1SK).BeginsWith("TODOITEM#")
        );

        var query = ddb.Query<TodoItemEntity>()
            .FromIndex(Gsi1IndexName)
            .WithKeyExpression(keyExpression)
            .WithLimit(limit)
            .BackwardSearch(true) // Note: (ScanIndexForward = false)
            .WithPaginationToken(paginationToken);

        if (isComplete.HasValue)
        {
            var filterExpression = Condition<TodoItemEntity>.On(x => x.IsCompleted).EqualTo(isComplete.Value);
            query = query.WithFilterExpression(filterExpression);
        }
        
        var page = await query.ToPageAsync();
        return page;
    }
}

// Converter class to convert between a ULID and DynamoDb string attribute value
public class UlidConverter : DdbConverter<Ulid>
{
    public override Ulid Read(in AttributeValue attributeValue)
    {
        var ulid = attributeValue.AsString();
        return Ulid.Parse(ulid);
    }

    public override AttributeValue Write(ref Ulid value)
    {
        return new StringAttributeValue(value.ToString());
    }
}