using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using Todo.Api.Endpoints;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class CreateTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestBase<Fixture>(fixture, output)
{
    [Fact]
    public async Task CreateTodoItemOk()
    {
        var request = Given.CreateTodoItemRequest(Ulid.NewUlid().ToString());
        
        var (apiResponse, response) = await Fixture.Client.POSTAsync<CreateTodoItemEndpoint, CreateTodoItemRequest, CreateTodoItemResponse>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        
        var entity = await Fixture.DdbStore.GetTodoItemAsync(response.TodoItem!.TenantId, response.TodoItem.TodoItemId, CancellationToken.None);
        entity.Should().NotBeNull();

        var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity!);
        response.TodoItem.Should().BeEquivalentTo(todoItemDto);
    }
    
    [Fact]
    public async Task CreateTodoItemIdempotent()
    {
        var request = Given.CreateTodoItemRequest(Ulid.NewUlid().ToString(), Ulid.NewUlid());
        
        var (apiResponse1, response1) = await Fixture.Client.POSTAsync<CreateTodoItemEndpoint, CreateTodoItemRequest, CreateTodoItemResponse>(request);
        
        apiResponse1.StatusCode.Should().Be(HttpStatusCode.OK);

        response1.Should().NotBeNull();
        response1!.TodoItem.Should().NotBeNull();
        
        // Note: Send the same request (with the same Idempotency Token) again
        var (apiResponse2, response2) = await Fixture.Client.POSTAsync<CreateTodoItemEndpoint, CreateTodoItemRequest, CreateTodoItemResponse>(request);
        
        apiResponse2.StatusCode.Should().Be(HttpStatusCode.OK);

        response2.Should().NotBeNull();
        response2!.TodoItem.Should().NotBeNull();

        response1.TodoItem.Should().BeEquivalentTo(response2.TodoItem);
    }

    [Fact]
    public async Task CreateTodoItemValidationFailure()
    {
        // Note: Missing Title and Description
        var request = new CreateTodoItemRequest
        {
            TenantId = Ulid.NewUlid().ToString()
        };

        var (apiResponse, response) = await Fixture.Client.POSTAsync<CreateTodoItemEndpoint, CreateTodoItemRequest, ErrorResponse>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Errors.Should().HaveCount(2);
    }
}