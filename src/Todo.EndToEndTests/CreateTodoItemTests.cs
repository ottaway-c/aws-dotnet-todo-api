using FluentAssertions;

namespace Todo.EndToEndTests;

public class CreateTodoItemTests
{
    [Fact]
    public async Task CreateTodoItemOk()
    {
        var fixture = await Fixture.Ensure();
        var client = fixture.Client;
        
        var now = DateTime.UtcNow;
        
        var request = Given.CreateTodoItemRequest();
        var response = await client.CreateTodoItemAsync(request);
        
        response.Should().NotBeNull();
        response.TodoItem.Should().NotBeNull();
        response.TodoItem.TodoItemId.Should().NotBeNull();
        response.TodoItem.TenantId.Should().Be(request.TenantId);
        response.TodoItem.IdempotencyToken.Should().NotBeNull();
        response.TodoItem.Title.Should().Be(request.Title);
        response.TodoItem.Notes.Should().Be(request.Notes);
        response.TodoItem.IsCompleted.Should().BeFalse();
        
        // NOTE: Allow for some variation in the clock skew.
        response.TodoItem.CreatedDate.Should().BeCloseTo(now, TimeSpan.FromMinutes(2));
        response.TodoItem.UpdatedDate.Should().BeCloseTo(now, TimeSpan.FromMinutes(2));
    }
}