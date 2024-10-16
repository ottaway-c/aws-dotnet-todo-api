using FluentAssertions;
using Xunit.Abstractions;

namespace Todo.EndToEndTests;

public class CreateTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestClass<Fixture>(fixture, output)
{
    [Fact]
    public async Task CreateTodoItem_Ok()
    {
        var tenantId = Given.TenantId();
        var client = Fixture.Client;
        
        var now = DateTime.UtcNow;
        
        var request = Given.CreateTodoItemRequest();
        var response = await client.V1.Tenant[tenantId].Todo.PostAsync(request);
        
        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        response.TodoItem!.TodoItemId.Should().NotBeNull();
        response.TodoItem.TenantId.Should().Be(tenantId.ToString());
        response.TodoItem.IdempotencyToken.Should().NotBeNull();
        response.TodoItem.Title.Should().Be(request.Title);
        response.TodoItem.Notes.Should().Be(request.Notes);
        response.TodoItem.IsCompleted.Should().BeFalse();
        
        // NOTE: Allow for some variation in the clock skew.
        response.TodoItem.CreatedDate.Should().BeCloseTo(now, TimeSpan.FromMinutes(2));
        response.TodoItem.UpdatedDate.Should().BeCloseTo(now, TimeSpan.FromMinutes(2));
    }
}