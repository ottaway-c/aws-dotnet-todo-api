using FluentAssertions;
using Xunit.Abstractions;

namespace Todo.EndToEndTests;

public class UpdateTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestClass<Fixture>(fixture, output)
{
    [Fact]
    public async Task UpdateTodoItem_Ok()
    {
        var tenantId = Given.TenantId();
        var client = Fixture.Client;

        var now = DateTime.UtcNow;

        var args = Given.CreateTodoItemArgs(tenantId);
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var request = Given.UpdateTodoItemRequest();
        var response = await client.V1.Tenant[tenantId].Todo[entity.TodoItemId.ToString()].PutAsync(request);

        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        response.TodoItem!.TodoItemId.Should().NotBeNull();
        response.TodoItem.TenantId.Should().Be(entity.TenantId);
        response.TodoItem.IdempotencyToken.Should().NotBeNull();
        response.TodoItem.Title.Should().Be(request.Title);
        response.TodoItem.Notes.Should().Be(request.Notes);
        response.TodoItem.IsCompleted.Should().BeTrue();

        // NOTE: Allow for some variation in the clock skew.
        response.TodoItem.CreatedDate.Should().BeCloseTo(now, TimeSpan.FromMinutes(2));
        response.TodoItem.UpdatedDate.Should().BeCloseTo(now, TimeSpan.FromMinutes(2));
    }
}
