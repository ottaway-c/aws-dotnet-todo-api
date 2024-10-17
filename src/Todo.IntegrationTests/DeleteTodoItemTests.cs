using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using Todo.Api;
using Todo.Api.Endpoints;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class DeleteTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestBase<Fixture>(fixture, output)
{
    [Fact]
    public async Task DeleteTodoItem_Ok()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var request = Given.DeleteTodoItemRequest(entity.TenantId, entity.TodoItemId);
        var httpResponse = await Fixture.Client.DELETEAsync<DeleteTodoItemEndpoint, DeleteTodoItemRequest>(request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Note:
        // Ensure the item is deleted from Dynamo
        entity = await Fixture.DdbStore.GetTodoItemAsync(entity.TenantId, entity.TodoItemId, CancellationToken.None);
        entity.Should().BeNull();
    }

    [Fact]
    public async Task DeleteTodoItem_NotFound()
    {
        var tenantId = Given.TenantId();
        var todoItemId = Ulid.NewUlid(); // Note: TodoItem does not exist

        var request = Given.DeleteTodoItemRequest(tenantId, todoItemId);
        var (httpResponse, response) = await Fixture.Client.DELETEAsync<DeleteTodoItemEndpoint, DeleteTodoItemRequest, ApiErrorResponse>(request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTodoItem_ValidationFailure()
    {
        var tenantId = Given.TenantId();

        // Note: Missing TodoItemId
        var request = new DeleteTodoItemRequest { TenantId = tenantId };

        var (httpResponse, response) = await Fixture.Client.DELETEAsync<DeleteTodoItemEndpoint, DeleteTodoItemRequest, ApiErrorResponse>(request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.ErrorCode.Should().Be("ValidationError");
        response.Errors.Should().HaveCount(1);
    }
}
