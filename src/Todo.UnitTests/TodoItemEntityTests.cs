using FluentAssertions;
using Todo.Core;
using Todo.Core.Entities;

namespace Todo.UnitTests;

public class TodoItemEntityTests
{
    [Fact]
    public void Map()
    {
        var args = new CreateTodoItemArgs
        {
            Title = "Put out the washing",
            Notes = "Reminder: Clothes are still in the machine!",
            TenantId = Ulid.NewUlid(),
            IdempotencyToken = Ulid.NewUlid()
        };

        var now = DateTime.UtcNow;

        var entity = TodoItemEntity.Create(args);
        
        entity.Should().NotBeNull();
        entity.TodoItemId.Should().NotBeNull();
        entity.PK.Should().BeEquivalentTo($"TENANT#{entity.TenantId}");
        entity.SK.Should().BeEquivalentTo($"TODOITEM#{entity.TodoItemId}");
        entity.Title.Should().BeEquivalentTo(args.Title);
        entity.Notes.Should().BeEquivalentTo(args.Notes);
        entity.IsCompleted.Should().BeFalse();
        entity.IdempotencyToken.Should().BeEquivalentTo(args.IdempotencyToken);
        entity.Entity.Should().BeEquivalentTo("TodoItem");
        entity.CreatedDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
        entity.UpdatedDate.Should().BeCloseTo(now, TimeSpan.FromSeconds(5));
        entity.CreatedDate.Should().Be(entity.UpdatedDate);
    }
}