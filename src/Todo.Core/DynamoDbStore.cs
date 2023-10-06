using EfficientDynamoDb;
using EfficientDynamoDb.Converters;
using EfficientDynamoDb.DocumentModel;
using EfficientDynamoDb.Exceptions;
using EfficientDynamoDb.Operations.Shared;
using Todo.Core.Entities;

namespace Todo.Core;

public class CreateTodoItemArgs
{
    public required string Title { get; init; }
    public required string Notes { get; init; }
    public required Ulid TenantId { get; init; }
    public required Ulid IdempotencyToken { get; init; }
}

public class UpdateTodoItemArgs
{
    public required Ulid TodoItemId { get; init; }
    public required Ulid TenantId { get; init; }
    public required string Title { get; init; }
    public required string Notes { get; init; }
    public required bool IsCompleted { get; init; }
}

public class DeleteTodoItemArgs
{
    public required Ulid TodoItemId { get; init; }
    public required Ulid TenantId { get; init; }
}

public interface IDynamoDbStore
{
    Task<TodoItemEntity> CreateTodoItemAsync(CreateTodoItemArgs args, CancellationToken ct);

    Task<TodoItemEntity?> UpdateTodoItemAsync(UpdateTodoItemArgs args, CancellationToken ct);

    Task<bool> DeleteTodoItemAsync(DeleteTodoItemArgs args, CancellationToken ct);

    Task<TodoItemEntity?> GetTodoItemAsync(Ulid tenantId, Ulid todoItemId, CancellationToken ct);

    Task<PagedResult<TodoItemEntity>> ListTodoItemsAsync(Ulid tenantId, int limit, bool? isComplete = null, string? paginationToken = null);
}

public class DynamoDbStore : IDynamoDbStore
{
    private readonly IDynamoDbContext _ddb;

    public DynamoDbStore(IDynamoDbContext ddb)
    {
        _ddb = ddb;
    }
    
    public async Task<TodoItemEntity> CreateTodoItemAsync(CreateTodoItemArgs args, CancellationToken ct)
    {
        var entity = TodoItemEntity.Create(args);
        var idempotencyEntity = IdempotencyEntity.Create(args, entity);

        var entityFilterExpression = Condition<TodoItemEntity>.On(x => x.PK).NotExists();
        var idempotencyEntityFilterExpression = Condition<IdempotencyEntity>.On(x => x.PK).NotExists();

        try
        {
            await _ddb.TransactWrite().WithItems(
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
                var duplicateEntity = _ddb.ToObject<IdempotencyEntity>(document!);

                entity = await GetTodoItemAsync(duplicateEntity.TenantId, duplicateEntity.TodoItemId, ct);
                if (entity == null) throw new InvalidOperationException("Failed to find duplicate TodoItem entity");
                
                return entity;
            }

            throw;
        }
    }

    public async Task<TodoItemEntity?> UpdateTodoItemAsync(UpdateTodoItemArgs args, CancellationToken ct)
    {
        var pk = $"TENANT#{args.TenantId}";
        var sk = $"TODOITEM#{args.TodoItemId}";
        var now = DateTime.UtcNow;
        
        var condition = Condition.ForEntity<TodoItemEntity>();
        var expression = Joiner.And(condition.On(x => x.PK).Exists(),
            condition.On(x => x.SK).Exists());

        try
        {
            var entity = await _ddb.UpdateItem<TodoItemEntity>()
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
        var pk = $"TENANT#{args.TenantId}";
        var sk = $"TODOITEM#{args.TodoItemId}";
        
        var condition = Condition.ForEntity<TodoItemEntity>();
        var expression = Joiner.And(condition.On(x => x.PK).Exists(),
            condition.On(x => x.SK).Exists());
        
        try
        {
            await _ddb.DeleteItem<TodoItemEntity>()
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

    public async Task<TodoItemEntity?> GetTodoItemAsync(Ulid tenantId, Ulid todoItemId, CancellationToken ct)
    {
        var pk = $"TENANT#{tenantId}";
        var sk = $"TODOITEM#{todoItemId}";
        
        var entity = await _ddb.GetItemAsync<TodoItemEntity>(pk, sk, ct);

        return entity;
    }

    public async Task<PagedResult<TodoItemEntity>> ListTodoItemsAsync(Ulid tenantId, int limit, bool? isComplete = null, string? paginationToken = null)
    {
        var keyCondition = Condition.ForEntity<TodoItemEntity>();
        var keyExpression = Joiner.And(
            keyCondition.On(x => x.PK).EqualTo($"TENANT#{tenantId}"),
            keyCondition.On(x => x.SK).BeginsWith("TODOITEM#")
        );

        var query = _ddb.Query<TodoItemEntity>()
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