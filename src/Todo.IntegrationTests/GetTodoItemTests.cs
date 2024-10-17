using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using Todo.Api;
using Todo.Api.Endpoints;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class GetTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestBase<Fixture>(fixture, output)
{
    [Fact]
    public async Task GetTodoItem_Ok()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var request = Given.GetTodoItemRequest(entity.TenantId, entity.TodoItemId);
        var (httpResponse, response) = await Fixture.Client.GETAsync<GetTodoItemEndpoint, GetTodoItemRequest, GetTodoItemResponse>(request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Should().NotBeNull();
        response.TodoItem.Should().NotBeNull();

        var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity!);
        response.TodoItem.Should().BeEquivalentTo(todoItemDto);
    }

    [Fact]
    public async Task GetTodoItem_NotFound()
    {
        var tenantId = Given.TenantId();
        var todoItemId = Ulid.NewUlid(); // Note: TodoItem does not exist

        var request = Given.GetTodoItemRequest(tenantId, todoItemId);
        var (httpResponse, response) = await Fixture.Client.GETAsync<GetTodoItemEndpoint, GetTodoItemRequest, ApiErrorResponse>(request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTodoItem_ValidationFailure()
    {
        var tenantId = Given.TenantId();

        // Note: Missing TodoItemId
        var request = new GetTodoItemRequest { TenantId = tenantId };

        var (httpResponse, response) = await Fixture.Client.GETAsync<GetTodoItemEndpoint, GetTodoItemRequest, ApiErrorResponse>(request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.ErrorCode.Should().Be("ValidationError");
        response.Errors.Should().HaveCount(1);
    }
}
