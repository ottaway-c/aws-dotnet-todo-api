using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using Todo.Api.Endpoints;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class DeleteTodoItemTests : TestClass<Fixture>
{
    public DeleteTodoItemTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task DeleteTodoItemOk()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var request = Given.DeleteTodoItemRequest(entity.TenantId, entity.TodoItemId);
        var apiResponse = await Fixture.Client.DELETEAsync<DeleteTodoItemEndpoint, DeleteTodoItemRequest>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Note:
        // Ensure the item is deleted from Dynamo
        entity = await Fixture.DdbStore.GetTodoItemAsync(entity.TenantId, entity.TodoItemId, CancellationToken.None);
        entity.Should().BeNull();
    }
    
    [Fact]
    public async Task DeleteTodoItemNotFound()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var request = Given.DeleteTodoItemRequest(entity.TenantId, Ulid.NewUlid()); // Note: TodoItem does not exist
        var apiResponse = await Fixture.Client.DELETEAsync<DeleteTodoItemEndpoint, DeleteTodoItemRequest>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeleteTodoItemValidationFailure()
    {
        // Note: Missing TodoItemId
        var request = new DeleteTodoItemRequest
        {
            TenantId = Ulid.NewUlid()
        }; 

        var (apiResponse, response) = await Fixture.Client.DELETEAsync<DeleteTodoItemEndpoint, DeleteTodoItemRequest, ErrorResponse>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Errors.Should().HaveCount(1);
    }
}