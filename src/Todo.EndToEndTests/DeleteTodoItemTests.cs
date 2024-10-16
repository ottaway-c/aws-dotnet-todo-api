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

        await client.V1.Tenant[entity.TenantId].Todo[entity.TodoItemId.ToString()].DeleteAsync();
    }
    
    [Fact]
    public async Task DeleteTodoItemNotFound()
    {
        var client = Fixture.Client;
        
        var args = Given.CreateTodoItemArgs();
        await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var todoItemId = Ulid.NewUlid(); // Note: Bogus todoitem id

        await client.V1.Tenant[args.TenantId].Todo[todoItemId.ToString()].DeleteAsync();
    }
}