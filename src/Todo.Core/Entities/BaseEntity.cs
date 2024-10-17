using EfficientDynamoDb.Attributes;

namespace Todo.Core.Entities;

[DynamoDbTable("todo-table")]
public class BaseEntity
{
    [DynamoDbProperty(nameof(PK), DynamoDbAttributeType.PartitionKey)]
    public required string PK { get; init; }

    [DynamoDbProperty(nameof(SK), DynamoDbAttributeType.SortKey)]
    public required string SK { get; init; }

    [DynamoDbProperty(nameof(Entity))]
    public virtual required string Entity { get; init; }

    [DynamoDbProperty(nameof(CreatedDate))]
    public required DateTime CreatedDate { get; init; }

    [DynamoDbProperty(nameof(UpdatedDate))]
    public required DateTime UpdatedDate { get; set; }
}
