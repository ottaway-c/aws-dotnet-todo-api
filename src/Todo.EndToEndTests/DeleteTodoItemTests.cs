using Todo.Client.Models;
using Xunit.Abstractions;

namespace Todo.EndToEndTests;

public class DeleteTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestClass<Fixture>(fixture, output)
{
    [Fact]
    public async Task DeleteTodoItem_Ok()
    {
        var tenantId = Given.TenantId();
        var client = Fixture.Client;

        var args = Given.CreateTodoItemArgs(tenantId);
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        await client.V1.Tenant[tenantId].Todo[entity.TodoItemId.ToString()].DeleteAsync();
    }

    [Fact]
    public async Task DeleteTodoItem_NotFound()
    {
        var tenantId = Given.TenantId();
        var todoItemId = Ulid.NewUlid(); // Note: Bogus todoitem id
        var client = Fixture.Client;

        await Assert.ThrowsAsync<ApiErrorResponse>(async () => await client.V1.Tenant[tenantId].Todo[todoItemId.ToString()].DeleteAsync());
    }
}
