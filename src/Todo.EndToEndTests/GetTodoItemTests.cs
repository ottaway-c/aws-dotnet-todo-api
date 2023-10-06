using FluentAssertions;

namespace Todo.EndToEndTests;

public class GetTodoItemTests
{
    [Fact]
    public async Task GetTodoItemOk()
    {
        var fixture = await Fixture.Ensure();
        var client = fixture.Client;
        
        var now = DateTime.UtcNow;
        
        var args = Given.CreateTodoItemArgs();
        var entity = await fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var response = await client.GetTodoItemAsync(entity.TenantId, entity.TodoItemId);
        
        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        response.TodoItem.TodoItemId.Should().NotBeNull();
        response.TodoItem.TenantId.Should().Be(entity.TenantId);
        response.TodoItem.IdempotencyToken.Should().NotBeNull();
        response.TodoItem.Title.Should().Be(args.Title);
        response.TodoItem.Notes.Should().Be(args.Notes);
        response.TodoItem.IsCompleted.Should().BeFalse();
        
        // NOTE: Allow for some variation in the clock skew.
        response.TodoItem.CreatedDate.Should().BeCloseTo(now, TimeSpan.FromMinutes(2));
        response.TodoItem.UpdatedDate.Should().BeCloseTo(now, TimeSpan.FromMinutes(2));
    }
    
    [Fact]
    public async Task GetTodoItemNotFound()
    {
        var fixture = await Fixture.Ensure();
        var client = fixture.Client;
        
        var args = Given.CreateTodoItemArgs();
        await fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var todoItemId = Ulid.NewUlid(); // Note: Bogus todoitem id
        
        var response = await client.GetTodoItemAsync(args.TenantId, todoItemId);
        
        response.Should().BeNull();
    }
}