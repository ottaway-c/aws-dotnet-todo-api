using FluentAssertions;
using Todo.Core.Entities;

namespace Todo.UnitTests;

public class IdempotencyEntityTests
{
    [Fact]
    public void Map()
    {
        var args = Given.CreateTodoItemArgs();
        
        var todoItemEntity = TodoItemEntity.Create(args);
        var entity = IdempotencyEntity.Create(args, todoItemEntity);
        
        entity.Should().NotBeNull();
        entity.TodoItemId.Should().BeEquivalentTo(todoItemEntity.TodoItemId);
        entity.IdempotencyToken.Should().BeEquivalentTo(args.IdempotencyToken);
        entity.PK.Should().BeEquivalentTo($"TENANT#{entity.TenantId}#IDEMPOTENCY#{args.IdempotencyToken}");
        entity.SK.Should().Contain($"IDEMPOTENCY#{args.IdempotencyToken}");
        entity.IdempotencyToken.Should().BeEquivalentTo(args.IdempotencyToken);
        entity.Entity.Should().BeEquivalentTo("Idempotency");
        entity.CreatedDate.Should().Be(todoItemEntity.CreatedDate);
        entity.UpdatedDate.Should().Be(todoItemEntity.UpdatedDate);
    }
}