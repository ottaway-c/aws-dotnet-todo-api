using System.Net;
using FastEndpoints;
using FluentAssertions;
using Todo.Api;
using Todo.Api.Endpoints;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class UpdateTodoItemTests(Fixture fixture, ITestOutputHelper output) : TestBase<Fixture>(fixture, output)
{
    [Fact]
    public async Task UpdateTodoItem_Ok()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var request = Given.UpdateTodoItemRequest(entity.TenantId, entity.TodoItemId);
        var (httpResponse, response) = await Fixture.Client.PUTAsync<UpdateTodoItemEndpoint, UpdateTodoItemRequest, UpdateTodoItemResponse>(request);
        
        httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        
        // Note: Fetch the updated item from Dynamo
        entity = await Fixture.DdbStore.GetTodoItemAsync(entity.TenantId, entity.TodoItemId, CancellationToken.None);
        
        var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity!);
        response.TodoItem.Should().BeEquivalentTo(todoItemDto);
    }
    
    [Fact]
    public async Task UpdateTodoItem_NotFound()
    {
        var tenantId = Given.TenantId();
        var todoItemId = Ulid.NewUlid();  // Note: TodoItem does not exist
        
        var request = Given.UpdateTodoItemRequest(tenantId, todoItemId);
        var (httpResponse, response) = await Fixture.Client.PUTAsync<UpdateTodoItemEndpoint, UpdateTodoItemRequest, ApiErrorResponse>(request);
        
        httpResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Should().NotBeNull();
    }
    
    [Fact]
    public async Task UpdateTodoItem_ValidationFailure()
    {
        var tenantId = Given.TenantId();
        var todoItemId = Ulid.NewUlid();
        
        var request = Given.UpdateTodoItemRequest(tenantId, todoItemId);
        request.TodoItemId = null; // Note: Invalidate

        var (httpResponse, response) = await Fixture.Client.PUTAsync<UpdateTodoItemEndpoint, UpdateTodoItemRequest, ApiErrorResponse>(request);
        
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.ErrorCode.Should().Be("ValidationError");
        response.Errors.Should().HaveCount(1);
    }
}
