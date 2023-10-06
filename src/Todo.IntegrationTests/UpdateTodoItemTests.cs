using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using Todo.Api.Endpoints;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class UpdateTodoItemTests : TestClass<Fixture>
{
    public UpdateTodoItemTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task UpdateTodoItemOk()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var request = Given.UpdateTodoItemRequest(entity.TenantId, entity.TodoItemId);
        var (apiResponse, response) = await Fixture.Client.PUTAsync<UpdateTodoItemEndpoint, UpdateTodoItemRequest, UpdateTodoItemResponse>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        
        // Note: Fetch the updated item from Dynamo
        entity = await Fixture.DdbStore.GetTodoItemAsync(entity.TenantId, entity.TodoItemId, CancellationToken.None);
        
        var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity!);
        response.TodoItem.Should().BeEquivalentTo(todoItemDto);
    }
    
    [Fact]
    public async Task UpdateTodoItemNotFound()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var request = Given.UpdateTodoItemRequest(entity.TenantId, Ulid.NewUlid()); // Note: TodoItem does not exist
        var apiResponse = await Fixture.Client.PUTAsync<UpdateTodoItemEndpoint, UpdateTodoItemRequest>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task UpdateTodoItemValidationFailure()
    {
        var request = Given.UpdateTodoItemRequest(Ulid.NewUlid(), Ulid.NewUlid());
        request.TodoItemId = null; // Note: Invalidate

        var (apiResponse, response) = await Fixture.Client.PUTAsync<UpdateTodoItemEndpoint, UpdateTodoItemRequest, ErrorResponse>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Errors.Should().HaveCount(1);
    }
}
