using System.ComponentModel;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class ListTodoItemsRequest
{
    [DefaultValue(Constants.DefaultTenantId)]
    public Ulid? TenantId { get; set; }
    
    [DefaultValue(25)]
    public int? Limit { get; set; }
    
    [DefaultValue("")]
    public string? PaginationToken { get; set; }
    
    public bool? IsCompleted { get; set; }
}

public class ListTodoItemsResponse
{
    public List<TodoItemDto> TodoItems { get; set; } = new();
    public string? PaginationToken { get; set; }
}

public class ListTodoItemsRequestValidator : Validator<ListTodoItemsRequest>
{
    public ListTodoItemsRequestValidator()
    {
        RuleFor(x => x.TenantId).NotNull();
        RuleFor(x => x.Limit).GreaterThanOrEqualTo(1).LessThanOrEqualTo(50);
    }
}

public class ListTodoItemsEndpoint : Endpoint<ListTodoItemsRequest, ListTodoItemsResponse>
{
    private readonly IDynamoDbStore _ddbStore;
    private readonly Mapper _mapper;
    
    public ListTodoItemsEndpoint(IDynamoDbStore ddbStore)
    {
        _ddbStore = ddbStore;
        _mapper = new Mapper();
    }
    
    public override void Configure()
    {
        Get("/api/{TenantId}/todo");
        Description(x => x.ProducesProblemFE<InternalErrorResponse>(500));
        Summary(s =>
        {
            s.Summary = "List TodoItems";
            s.Description = "List and filter TodoItems";
            s.RequestParam(r => r.TenantId!, "Tenant Id");
            s.RequestParam(r => r.Limit!, "The maximum number of items to return in a single query. Optional, the default is 25");
            s.RequestParam(r => r.PaginationToken!, "The pagination token containing the next range of results. Optional");
            s.RequestParam(r => r.IsCompleted!, "Flag to filter on completed/non completed items. Optional");
            
        });
        Validator<ListTodoItemsRequestValidator>();
    }
    
    public override async Task HandleAsync(ListTodoItemsRequest request, CancellationToken ct)
    {
        // Note: Assign a default limit if the request does not contain one
        request.Limit ??= 25;
        
        var page = await _ddbStore.ListTodoItemsAsync(request.TenantId!.Value, request.Limit.Value, request.IsCompleted, request.PaginationToken);
        if (!page.Items.Any())
        {
            Response.TodoItems = new List<TodoItemDto>();
            return;
        }
        
        var items = page.Items.Select(x =>
        {
            var todoItem = _mapper.TodoItemEntityToDto(x);
            return todoItem;
        }).ToList();

        Response.TodoItems = items;
        Response.PaginationToken = page.PaginationToken;
    }
}