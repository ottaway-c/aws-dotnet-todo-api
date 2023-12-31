﻿using System.ComponentModel;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class GetTodoItemRequest
{
    [DefaultValue(Constants.DefaultTodoItemId)]
    public Ulid? TodoItemId { get; set; }
    
    [DefaultValue(Constants.DefaultTenantId)]
    public Ulid? TenantId { get; set; }
}

public class GetTodoItemResponse
{
    public TodoItemDto? TodoItem { get; set; }
}

public class GetTodoItemRequestValidator : Validator<GetTodoItemRequest>
{
    public GetTodoItemRequestValidator()
    {
        RuleFor(x => x.TodoItemId).NotNull();
        RuleFor(x => x.TenantId).NotNull();
    }
}

public class GetTodoItemEndpoint : Endpoint<GetTodoItemRequest, GetTodoItemResponse>
{
    private readonly IDynamoDbStore _ddbStore;
    private readonly Mapper _mapper;
    
    public GetTodoItemEndpoint(IDynamoDbStore ddbStore)
    {
        _ddbStore = ddbStore;
        _mapper = new Mapper();
    }
    
    public override void Configure()
    {
        Get("/api/{TenantId}/todo/{TodoItemId}");
        Description(x => x.Produces(404));
        Description(x => x.ProducesProblemFE<InternalErrorResponse>(500));
        Summary(s =>
        {
            s.Summary = "Get TodoItem";
            s.Description = "Get a TodoItem";
            s.RequestParam(r => r.TodoItemId!, "TodoItem Id");
            s.RequestParam(r => r.TenantId!, "Tenant Id");
        });
        Validator<GetTodoItemRequestValidator>();
    }
    
    public override async Task HandleAsync(GetTodoItemRequest request, CancellationToken ct)
    {
        var entity = await _ddbStore.GetTodoItemAsync(request.TenantId!.Value, request.TodoItemId!.Value, ct);
        
        if (entity == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }
        
        var todoItemDto = _mapper.TodoItemEntityToDto(entity);

        Response.TodoItem = todoItemDto;
    }
}