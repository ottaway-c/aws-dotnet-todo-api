using System.ComponentModel;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class UpdateTodoItemRequest
{
    [DefaultValue(Constants.DefaultTodoItemId)]
    public Ulid? TodoItemId { get; set; }
    
    [DefaultValue(Constants.DefaultTenantId)]
    public Ulid? TenantId { get; set; }
    
    [DefaultValue(Constants.DefaultTitle)]
    public string? Title { get; set; }
    
    [DefaultValue(Constants.DefaultNotes)]
    public string? Notes { get; set; }
    
    [DefaultValue(true)]
    public bool? IsCompleted { get; set; }
}

public class UpdateTodoItemResponse
{
    public TodoItemDto? TodoItem { get; set; }
}

public class UpdateTodoItemRequestValidator : Validator<UpdateTodoItemRequest>
{
    public UpdateTodoItemRequestValidator()
    {
        RuleFor(x => x.TodoItemId).NotNull();
        RuleFor(x => x.TenantId).NotNull();
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Notes).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.IsCompleted).NotNull();
    }
}

public class UpdateTodoItemEndpoint : Endpoint<UpdateTodoItemRequest, UpdateTodoItemResponse>
{
    private readonly IDynamoDbStore _ddbStore;
    private readonly Mapper _mapper;
    
    public UpdateTodoItemEndpoint(IDynamoDbStore ddbStore)
    {
        _ddbStore = ddbStore;
        _mapper = new Mapper();
    }
    
    public override void Configure()
    {
        Put("/api/{TenantId}/todo/{TodoItemId}");
        Description(x => x.Produces(404));
        Description(x => x.ProducesProblemFE<InternalErrorResponse>(500));
        Summary(s =>
        {
            s.Summary = "Update TodoItem";
            s.Description = "Update a TodoItem";
            s.RequestParam(r => r.TenantId!, "Tenant Id");
            s.RequestParam(r => r.TodoItemId!, "TodoItem Id");
            
        });
        Validator<UpdateTodoItemRequestValidator>();
    }
    
    public override async Task HandleAsync(UpdateTodoItemRequest request, CancellationToken ct)
    {
        var args = _mapper.UpdateTodoItemRequestToArgs(request);
        var entity = await _ddbStore.UpdateTodoItemAsync(args, ct);
    
        if (entity == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
    
        var todoItemDto = _mapper.TodoItemEntityToDto(entity);

        Response.TodoItem = todoItemDto;
    }
}