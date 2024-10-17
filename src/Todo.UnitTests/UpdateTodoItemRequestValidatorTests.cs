using FluentValidation.TestHelper;
using Todo.Api.Endpoints;

namespace Todo.UnitTests;

public class UpdateTodoItemRequestValidatorTests
{
    private readonly UpdateTodoItemRequestValidator _validator = new();

    [Fact]
    public void ShouldNotHaveErrorsForValidRequest()
    {
        var request = Given.UpdateTodoItemRequest();

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TodoItemIdIsRequired()
    {
        var request = new UpdateTodoItemRequest { TodoItemId = null };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TodoItemId);
    }

    [Fact]
    public void TenantIdIsRequired()
    {
        var request = new UpdateTodoItemRequest { TenantId = null };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Theory]
    [ClassData(typeof(Given.TitleTestData))]
    public void TitleIsRequired(string? value)
    {
        var request = new UpdateTodoItemRequest { Title = value };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [ClassData(typeof(Given.NotesTestData))]
    public void NotesAreRequired(string? value)
    {
        var request = new UpdateTodoItemRequest { Notes = value };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void IsCompletedIsRequired()
    {
        var request = new UpdateTodoItemRequest { IsCompleted = null };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.IsCompleted);
    }
}
