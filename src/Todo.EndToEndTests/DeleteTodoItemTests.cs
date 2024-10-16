using FluentAssertions;
using Xunit.Abstractions;

namespace Todo.EndToEndTests;

public class DeleteTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestClass<Fixture>(fixture, output)
{
    [Fact]
    public async Task DeleteTodoItemOk()
    {
        var client = Fixture.Client;
        
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var response = await client.DeleteTodoItemAsync(entity.TenantId, entity.TodoItemId);
        response.Should().BeTrue();
    }
    
    [Fact]
    public async Task DeleteTodoItemNotFound()
    {
        var client = Fixture.Client;
        
        var args = Given.CreateTodoItemArgs();
        await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var todoItemId = Ulid.NewUlid(); // Note: Bogus todoitem id

        var response = await client.DeleteTodoItemAsync(args.TenantId, todoItemId);
        response.Should().BeFalse();
    }
}