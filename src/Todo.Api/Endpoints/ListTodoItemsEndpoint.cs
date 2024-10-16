using Microsoft.AspNetCore.Http.HttpResults;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class ListTodoItemsRequest : ITenantId
{
    public string? TenantId { get; set; }
    public int? Limit { get; set; }
    public string? PaginationToken { get; set; }
    public bool? IsCompleted { get; set; }
}

public class ListTodoItemsResponse
{
    public required List<TodoItemDto> TodoItems { get; init; }
    public required string? PaginationToken { get; init; }
}

public class ListTodoItemsRequestValidator : Validator<ListTodoItemsRequest>
{
    public ListTodoItemsRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Limit).GreaterThanOrEqualTo(1).LessThanOrEqualTo(50);
    }
}

public class ListTodoItemsEndpoint(IDynamoDbStore ddbStore, Mapper mapper)
    : Endpoint<ListTodoItemsRequest, Ok<ListTodoItemsResponse>>
{
    public override void Configure()
    {
        Get("tenant/{TenantId}/todo");
        Version(1);
        Summary(s =>
        {
            s.Summary = "List TodoItems";
            s.Description = "List and filter TodoItems";
            
        });
        Validator<ListTodoItemsRequestValidator>();
    }

    public override async Task<Ok<ListTodoItemsResponse>> ExecuteAsync(ListTodoItemsRequest request, CancellationToken ct)
    {
        // Note: Assign a default limit if the request does not contain one
        request.Limit ??= 25;
        
        var page = await ddbStore.ListTodoItemsAsync(request.TenantId!, request.Limit.Value, request.IsCompleted, request.PaginationToken);
        if (page.Items.Count == 0)
        {
            return TypedResults.Ok(new ListTodoItemsResponse
            {
                TodoItems = [],
                PaginationToken = null
            });
        }
        
        var items = page.Items.Select(x =>
        {
            var todoItem = mapper.TodoItemEntityToDto(x);
            return todoItem;
        }).ToList();

        var response = new ListTodoItemsResponse
        {
            TodoItems = items,
            PaginationToken = page.PaginationToken
        };

        return TypedResults.Ok(response);
    }
}