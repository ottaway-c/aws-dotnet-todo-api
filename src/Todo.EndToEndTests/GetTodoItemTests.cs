using FluentAssertions;
using Xunit.Abstractions;

namespace Todo.EndToEndTests;

public class GetTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestClass<Fixture>(fixture, output)
{
    [Fact]
    public async Task GetTodoItemOk()
    {
        var client = Fixture.Client;
        
        var now = DateTime.UtcNow;
        
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var response = await client.V1.Tenant[entity.TenantId].Todo[entity.TodoItemId.ToString()].GetAsync();
        
        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        response.TodoItem!.TodoItemId.Should().NotBeNull();
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
        var client = Fixture.Client;
        
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var todoItemId = Ulid.NewUlid(); // Note: Bogus todoitem id
        
        await client.V1.Tenant[entity.TenantId].Todo[todoItemId.ToString()].GetAsync();
        
    }
}