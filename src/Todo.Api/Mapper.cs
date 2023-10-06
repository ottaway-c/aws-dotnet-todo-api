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
    public partial TodoItemDto TodoItemEntityToDto(TodoItemEntity entity);
}