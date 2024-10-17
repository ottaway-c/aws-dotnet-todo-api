using FluentValidation.TestHelper;
using Todo.Api.Endpoints;

namespace Todo.UnitTests;

public class GetTodoItemRequestValidatorTests
{
    private readonly GetTodoItemRequestValidator _validator = new();

    [Fact]
    public void ShouldNotHaveErrorsForValidRequest()
    {
        var request = Given.GetTodoItemRequest();

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TodoItemIdIsRequired()
    {
        var request = Given.GetTodoItemRequest();
        request.TodoItemId = null; // Note: Invalidate

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TodoItemId);
    }

    [Fact]
    public void TenantIdIsRequired()
    {
        var request = Given.GetTodoItemRequest();
        request.TenantId = null; // Note: Invalidate

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }
}
