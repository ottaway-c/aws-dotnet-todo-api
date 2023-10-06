using FluentAssertions;
using Todo.Core.Entities;
using Mapper = Todo.Api.Mapper;

namespace Todo.UnitTests;

public class MapperTests
{
    private readonly Mapper _mapper = new();
    
    [Fact]
    public void MapTodoItemEntityToDto()
    {
        var now = DateTime.UtcNow;
        
        var entity = new TodoItemEntity
        {
            PK = "",
            SK = "",
            TodoItemId = Ulid.NewUlid(),
            Title = "Put out the washing",
            Notes = "Reminder: Clothes are still in the machine!",
            IsCompleted = false,
            TenantId = Ulid.NewUlid(),
            IdempotencyToken = Ulid.NewUlid(),
            CreatedDate = now,
            UpdatedDate = now,
            Entity = "TodoItem"
        };
        
        var dto = _mapper.TodoItemEntityToDto(entity);

        dto.Should().NotBeNull();
        dto.TodoItemId.Should().BeEquivalentTo(entity.TodoItemId);
        dto.TenantId.Should().BeEquivalentTo(entity.TenantId);
        dto.IdempotencyToken.Should().BeEquivalentTo(entity.IdempotencyToken);
        dto.Title.Should().BeEquivalentTo(entity.Title);
        dto.Notes.Should().BeEquivalentTo(entity.Notes);
        dto.IsCompleted.Should().Be(entity.IsCompleted);
        dto.CreatedDate.Should().Be(entity.CreatedDate);
        dto.UpdatedDate.Should().Be(entity.UpdatedDate);
    }

    [Fact]
    public void MapCreateTodoItemRequestToArgs()
    {
        var request = Given.CreateTodoItemRequest();
        var args = _mapper.CreateTodoItemRequestToArgs(request);

        args.Should().NotBeNull();
        args.Title.Should().BeEquivalentTo(request.Title);
        args.Notes.Should().BeEquivalentTo(request.Notes);
        args.IdempotencyToken.Should().BeEquivalentTo(request.IdempotencyToken);
    }

    [Fact]
    public void MapUpdateTodoItemRequestToArgs()
    {
        var request = Given.UpdateTodoItemRequest();
        var args = _mapper.UpdateTodoItemRequestToArgs(request);
        
        args.Should().NotBeNull();
        args.TodoItemId.Should().BeEquivalentTo(request.TodoItemId);
        args.TenantId.Should().BeEquivalentTo(request.TenantId);
        args.Title.Should().BeEquivalentTo(request.Title);
        args.Notes.Should().BeEquivalentTo(request.Notes);
        args.IsCompleted.Should().Be(request.IsCompleted!.Value);
    }

    [Fact]
    public void MapDeleteTodoItemRequestToArgs()
    {
        var request = Given.DeleteTodoItemRequest();
        var args = _mapper.DeleteTodoItemRequestToArgs(request);

        args.Should().NotBeNull();
        args.TodoItemId.Should().BeEquivalentTo(request.TodoItemId);
        args.TenantId.Should().BeEquivalentTo(request.TenantId);
    }
}