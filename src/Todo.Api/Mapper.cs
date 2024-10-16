using Riok.Mapperly.Abstractions;
using Todo.Api.Endpoints;
using Todo.Core;
using Todo.Core.Entities;

namespace Todo.Api;

[Mapper]
public partial class Mapper
{
    public partial CreateTodoItemArgs CreateTodoItemRequestToArgs(CreateTodoItemRequest request);
    public partial UpdateTodoItemArgs UpdateTodoItemRequestToArgs(UpdateTodoItemRequest request);
    public partial DeleteTodoItemArgs DeleteTodoItemRequestToArgs(DeleteTodoItemRequest request);
    
    [MapperIgnoreSource(nameof(TodoItemEntity.PK))]
    [MapperIgnoreSource(nameof(TodoItemEntity.SK))]
    [MapperIgnoreSource(nameof(TodoItemEntity.GSI1PK))]
    [MapperIgnoreSource(nameof(TodoItemEntity.GSI1SK))]
    [MapperIgnoreSource(nameof(TodoItemEntity.Entity))]
    public partial TodoItemDto TodoItemEntityToDto(TodoItemEntity entity);
}