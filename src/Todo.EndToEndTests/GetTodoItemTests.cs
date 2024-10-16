using FluentAssertions;
using Todo.Client.Models;
using Xunit.Abstractions;

namespace Todo.EndToEndTests;

public class GetTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestClass<Fixture>(fixture, output)
{
    [Fact]
    public async Task GetTodoItem_Ok()
    {
        var tenantId = Given.TenantId();
        var client = Fixture.Client;
        
        var now = DateTime.UtcNow;
        
        var args = Given.CreateTodoItemArgs(tenantId);
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var response = await client.V1.Tenant[tenantId].Todo[entity.TodoItemId.ToString()].GetAsync();
        
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
    public async Task GetTodoItem_NotFound()
    {
        var tenantId = Given.TenantId();
        var todoItemId = Ulid.NewUlid(); // Note: Bogus todoitem id
        var client = Fixture.Client;
        
       await Assert.ThrowsAsync<ApiErrorResponse>(async () => await client.V1.Tenant[tenantId].Todo[todoItemId.ToString()].GetAsync());
    }
}