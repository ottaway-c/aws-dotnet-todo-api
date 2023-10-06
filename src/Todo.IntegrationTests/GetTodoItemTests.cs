using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using Todo.Api.Endpoints;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class GetTodoItemTests : TestClass<Fixture>
{
    public GetTodoItemTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task GetTodoItemOk()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

        var request = Given.GetTodoItemRequest(entity.TenantId, entity.TodoItemId);
        var (apiResponse, response) = await Fixture.Client.GETAsync<GetTodoItemEndpoint, GetTodoItemRequest, GetTodoItemResponse>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Should().NotBeNull();
        response!.TodoItem.Should().NotBeNull();
        
        var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity!);
        response.TodoItem.Should().BeEquivalentTo(todoItemDto);
    }
    
    [Fact]
    public async Task GetTodoItemNotFound()
    {
        var args = Given.CreateTodoItemArgs();
        var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        
        var request = Given.GetTodoItemRequest(entity.TenantId, Ulid.NewUlid()); // Note: TodoItem does not exist
        var apiResponse = await Fixture.Client.GETAsync<GetTodoItemEndpoint, GetTodoItemRequest>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetTodoItemValidationFailure()
    {
        // Note: Missing TodoItemId
        var request = new GetTodoItemRequest
        {
            TenantId = Ulid.NewUlid()
        }; 

        var (apiResponse, response) = await Fixture.Client.GETAsync<GetTodoItemEndpoint, GetTodoItemRequest, ErrorResponse>(request);
        
        apiResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        response.Should().NotBeNull();
        response!.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.Errors.Should().HaveCount(1);
    }
}