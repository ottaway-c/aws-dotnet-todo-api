using FluentAssertions;

namespace Todo.EndToEndTests;

public class DeleteTodoItemTests
{
    [Fact]
    public async Task DeleteTodoItemOk()
    {
        var fixture = await Fixture.Ensure();
        var client = fixture.Client;
        
        var args = Given.CreateTodoItemArgs();
        var entity = await fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var response = await client.DeleteTodoItemAsync(entity.TenantId, entity.TodoItemId);
        response.Should().BeTrue();
    }
    
    [Fact]
    public async Task DeleteTodoItemNotFound()
    {
        var fixture = await Fixture.Ensure();
        var client = fixture.Client;
        
        var args = Given.CreateTodoItemArgs();
        await fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var todoItemId = Ulid.NewUlid(); // Note: Bogus todoitem id

        var response = await client.DeleteTodoItemAsync(args.TenantId, todoItemId);
        response.Should().BeFalse();
    }
}