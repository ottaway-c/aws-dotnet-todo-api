using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using Todo.Api;
using Todo.Api.Endpoints;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class CreateTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestBase<Fixture>(fixture, output)
{
    [Fact]
    public async Task CreateTodoItem_Ok()
    {
        var tenantId = Given.TenantId();
        var request = Given.CreateTodoItemRequest(tenantId);
        
        var (httpResponse, response) = await Fixture.Client.POSTAsync<CreateTodoItemEndpoint, CreateTodoItemRequest, CreateTodoItemResponse>(request);
        
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        
        var entity = await Fixture.DdbStore.GetTodoItemAsync(response.TodoItem.TenantId, response.TodoItem.TodoItemId, CancellationToken.None);
        entity.Should().NotBeNull();

        var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity!);
        response.TodoItem.Should().BeEquivalentTo(todoItemDto);
    }
    
    [Fact]
    public async Task CreateTodoItem_Idempotent()
    {
        var tenantId = Given.TenantId();
        var idempotencyToken = Ulid.NewUlid();
        
        var request = Given.CreateTodoItemRequest(tenantId, idempotencyToken);
        var (httpResponse1, response1) = await Fixture.Client.POSTAsync<CreateTodoItemEndpoint, CreateTodoItemRequest, CreateTodoItemResponse>(request);
        
        httpResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        response1.Should().NotBeNull();
        response1.TodoItem.Should().NotBeNull();
        
        // Note: Send the same request (with the same Idempotency Token) again
        var (httpResponse2, response2) = await Fixture.Client.POSTAsync<CreateTodoItemEndpoint, CreateTodoItemRequest, CreateTodoItemResponse>(request);
        
        httpResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        response2.Should().NotBeNull();
        response2.TodoItem.Should().NotBeNull();
        response1.TodoItem.Should().BeEquivalentTo(response2.TodoItem);
    }

    [Fact]
    public async Task CreateTodoItem_ValidationFailure()
    {
        var tenantId = Given.TenantId();
        
        // Note: Missing Title and Description
        var request = new CreateTodoItemRequest
        {
            TenantId = tenantId
        };

        var (httpResponse, response) = await Fixture.Client.POSTAsync<CreateTodoItemEndpoint, CreateTodoItemRequest, ApiErrorResponse>(request);
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.ErrorCode.Should().Be("ValidationError");
        response.Errors.Should().HaveCount(2);
    }
}