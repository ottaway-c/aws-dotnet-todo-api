﻿using System.ComponentModel;
using Todo.Core;

namespace Todo.Api.Endpoints;

public class CreateTodoItemRequest
{
    [DefaultValue(Constants.DefaultTitle)]
    public string? Title { get; init; }
    
    [DefaultValue(Constants.DefaultNotes)]
    public string? Notes { get; init; }
    
    [DefaultValue(Constants.DefaultTenantId)]
    public Ulid? TenantId { get; init; }
    
    [DefaultValue(Constants.DefaultIdempotencyToken)]
    public Ulid? IdempotencyToken { get; set; }
}

public class CreateTodoItemResponse
{
    public TodoItemDto? TodoItem { get; set; }
}

public class CreateTodoItemRequestValidator : Validator<CreateTodoItemRequest>
{
    public CreateTodoItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.Notes).NotEmpty().MinimumLength(3).MaximumLength(100);
        RuleFor(x => x.TenantId).NotNull();
    }
}

public class CreateTodoItemEndpoint : Endpoint<CreateTodoItemRequest, CreateTodoItemResponse>
{
    private readonly IDynamoDbStore _ddb;
    private readonly Mapper _mapper;

    public CreateTodoItemEndpoint(IDynamoDbStore ddb)
    {
        _ddb = ddb;
        _mapper = new Mapper();
    }
    
    public override void Configure()
    {
        Post("/api/{TenantId}/todo/");
        Description(x => x.ProducesProblemFE<InternalErrorResponse>(500));
        Summary(s =>
        {
            s.Summary = "Create TodoItem";
            s.Description = "Create a new TodoItem";
            s.RequestParam(r => r.TenantId!, "Tenant Id");
        });
        Validator<CreateTodoItemRequestValidator>();
    }

    public override async Task HandleAsync(CreateTodoItemRequest request, CancellationToken ct)
    {
        // Note: Assign an idempotency token if the request does not contain one
        request.IdempotencyToken ??= Ulid.NewUlid();

        var args = _mapper.CreateTodoItemRequestToArgs(request);
        var entity = await _ddb.CreateTodoItemAsync(args, ct);
        
        Response.TodoItem = _mapper.TodoItemEntityToDto(entity);
    }
}