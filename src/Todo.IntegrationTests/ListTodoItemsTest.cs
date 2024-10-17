using System.Net;
using FastEndpoints;
using FastEndpoints.Testing;
using FluentAssertions;
using FluentAssertions.Extensions;
using Todo.Api;
using Todo.Api.Endpoints;
using Todo.Core;
using Todo.Core.Entities;
using Xunit.Abstractions;

namespace Todo.IntegrationTests;

public class ListTodoItemsTest(Fixture fixture, ITestOutputHelper output) : TestBase<Fixture>(fixture, output)
{
    [Fact]
    public async Task ListTodoItemsBasic_Ok()
    {
        var tenantId = Given.TenantId();
        int total = 10;

        var entities = new List<TodoItemEntity>();

        {
            for (var i = 0; i < total; i++)
            {
                var args = Given.CreateTodoItemArgs(tenantId);
                var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

                entities.Add(entity);
            }
        }

        {
            var args = Given.CreateTodoItemArgs(); // Note: Different Tenant Id
            await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        }

        // NOTE: Dynamo returns the newest items first (ScanIndexForward = false)
        // We need to reverse the list of entities so that the items are sorted in descending order.
        entities.Reverse();

        Func<Task> asyncRetry = async () =>
        {
            // Note: The default API behaviour is to return 25 records
            var request = Given.ListTodoItemsRequest(tenantId);
            var (httpResponse, response) = await Fixture.Client.GETAsync<ListTodoItemsEndpoint, ListTodoItemsRequest, ListTodoItemsResponse>(request);

            httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            response.Should().NotBeNull();
            response!.TodoItems.Should().NotBeNull();

            response.TodoItems.Count.Should().Be(total);
            response.TodoItems.Should().OnlyHaveUniqueItems();

            foreach (var entity in entities.ToList())
            {
                var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity);
                response.TodoItems.Should().ContainEquivalentOf(todoItemDto);
            }
        };

        await asyncRetry.Should().NotThrowAfterAsync(waitTime: 5.Seconds(), pollInterval: 1.Seconds());
    }

    [Fact]
    public async Task ListTodoItemsWithPagination_Ok()
    {
        var tenantId = Given.TenantId();

        int total = 10;
        var limit = 4;
        var page = 0;
        string? paginationToken = null;

        var entities = new List<TodoItemEntity>();

        {
            for (var i = 0; i < total; i++)
            {
                var args = Given.CreateTodoItemArgs(tenantId);
                var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

                entities.Add(entity);
            }
        }

        {
            var args = Given.CreateTodoItemArgs(); // Note: Different Tenant Id
            await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        }

        // NOTE: Dynamo returns the newest items first (ScanIndexForward = false)
        // We need to reverse the list of entities so that the items are sorted in descending order.
        entities.Reverse();

        Func<Task> asyncRetry = async () =>
        {
            do
            {
                var request = Given.ListTodoItemsRequest(tenantId, limit: limit, paginationToken: paginationToken);
                var (httpResponse, response) = await Fixture.Client.GETAsync<ListTodoItemsEndpoint, ListTodoItemsRequest, ListTodoItemsResponse>(request);

                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                response.Should().NotBeNull();
                response!.TodoItems.Should().NotBeNull();

                if (response.PaginationToken != null)
                {
                    response.TodoItems.Count.Should().Be(limit);
                }
                else
                {
                    response.TodoItems.Count.Should().BeLessThan(limit);
                }

                response.TodoItems.Should().OnlyHaveUniqueItems();

                foreach (var entity in entities.Skip(page * limit).Take(limit).ToList())
                {
                    var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity);
                    response.TodoItems.Should().ContainEquivalentOf(todoItemDto);
                }

                paginationToken = response.PaginationToken;
                page++;
            } while (paginationToken != null);
        };

        await asyncRetry.Should().NotThrowAfterAsync(waitTime: 5.Seconds(), pollInterval: 1.Seconds());

        page.Should().Be(3);
    }

    [Fact]
    public async Task ListTodoItemsWithEmptyResponse_Ok()
    {
        var tenantId = Given.TenantId();

        var request = Given.ListTodoItemsRequest(tenantId);
        var (apiResponse, response) = await Fixture.Client.GETAsync<ListTodoItemsEndpoint, ListTodoItemsRequest, ListTodoItemsResponse>(request);

        apiResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Should().NotBeNull();
        response!.TodoItems.Should().NotBeNull();
        response.TodoItems.Should().BeEmpty();
    }

    [Fact]
    public async Task ListTodoItemsWithFilter_Ok()
    {
        var tenantId = Given.TenantId();

        int total = 10;
        var limit = 4;
        var page = 0;
        var itemCount = 0;
        string? paginationToken = null;

        var entities = new List<TodoItemEntity>();

        {
            for (var i = 0; i < total; i++)
            {
                var args = Given.CreateTodoItemArgs(tenantId);
                await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
            }
        }

        {
            for (var i = 0; i < total; i++)
            {
                var args = Given.CreateTodoItemArgs(tenantId);
                var entity = await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);

                var updateArgs = new UpdateTodoItemArgs
                {
                    TenantId = entity.TenantId,
                    TodoItemId = entity.TodoItemId,
                    Title = entity.Title,
                    Notes = entity.Notes,
                    IsCompleted =
                        true // Note: Set item to completed
                    ,
                };

                entity = await Fixture.DdbStore.UpdateTodoItemAsync(updateArgs, CancellationToken.None);

                entities.Add(entity!);
            }
        }

        // NOTE: Dynamo returns the newest items first (ScanIndexForward = false)
        // We need to reverse the list of entities so that the items are sorted in descending order.
        entities.Reverse();

        Func<Task> asyncRetry = async () =>
        {
            do
            {
                var request = Given.ListTodoItemsRequest(tenantId, limit: limit, paginationToken: paginationToken, isCompleted: true);
                var (httpResponse, response) = await Fixture.Client.GETAsync<ListTodoItemsEndpoint, ListTodoItemsRequest, ListTodoItemsResponse>(request);

                httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                response.Should().NotBeNull();
                response.TodoItems.Should().NotBeNull();

                itemCount += response.TodoItems.Count;

                if (response.PaginationToken != null && itemCount < total)
                {
                    response.TodoItems.Count.Should().Be(limit);
                }
                else
                {
                    response.TodoItems.Count.Should().BeLessThan(limit);
                }

                response.TodoItems.Should().OnlyHaveUniqueItems();

                foreach (var entity in entities.Skip(page * limit).Take(limit).ToList())
                {
                    var todoItemDto = Fixture.Mapper.TodoItemEntityToDto(entity);
                    response.TodoItems.Should().ContainEquivalentOf(todoItemDto);
                }

                paginationToken = response.PaginationToken;
                page++;
            } while (paginationToken != null);
        };

        await asyncRetry.Should().NotThrowAfterAsync(waitTime: 5.Seconds(), pollInterval: 1.Seconds());

        page.Should().Be(4);
    }

    [Fact]
    public async Task ListTodoItems_ValidationFailure()
    {
        var tenantId = Given.TenantId();
        var request = Given.ListTodoItemsRequest(tenantId, limit: 51); // Note: Limit is outside the upper bound

        var (httpResponse, response) = await Fixture.Client.GETAsync<ListTodoItemsEndpoint, ListTodoItemsRequest, ApiErrorResponse>(request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Should().NotBeNull();
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        response.ErrorCode.Should().Be("ValidationError");
        response.Errors.Should().HaveCount(1);
    }
}
