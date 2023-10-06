using FluentValidation.TestHelper;
using Todo.Api.Endpoints;

namespace Todo.UnitTests;

public class DeleteTodoItemRequestValidatorTests
{
    private readonly DeleteTodoItemRequestValidator _validator = new();
    
    [Fact]
    public void ShouldNotHaveErrorsForValidRequest()
    {
        var request = Given.DeleteTodoItemRequest();

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
    
    [Fact]
    public void TodoItemIdIsRequired()
    {
        var request = Given.DeleteTodoItemRequest();
        request.TodoItemId = null; // Note: Invalidate

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TodoItemId);
    }
    
    [Fact]
    public void TenantIdIsRequired()
    {
        var request = Given.DeleteTodoItemRequest();
        request.TenantId = null; // Note: Invalidate

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }
}