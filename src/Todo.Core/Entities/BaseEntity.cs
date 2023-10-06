using EfficientDynamoDb.Attributes;

namespace Todo.Core.Entities;

[DynamoDbTable("todo-table")]
public class BaseEntity
{
    [DynamoDbProperty(nameof(PK), DynamoDbAttributeType.PartitionKey)]
    public required string PK { get; set; }

    [DynamoDbProperty(nameof(SK), DynamoDbAttributeType.SortKey)]
    public required string SK { get; set; }

    [DynamoDbProperty(nameof(Entity))]
    public virtual required string Entity { get; set; }

    [DynamoDbProperty(nameof(CreatedDate))]
    public required DateTime CreatedDate { get; set; }

    [DynamoDbProperty(nameof(UpdatedDate))]
    public required DateTime UpdatedDate { get; set; }
}