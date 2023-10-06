using FluentValidation.TestHelper;
using Todo.Api.Endpoints;

namespace Todo.UnitTests;

public class ListTodoItemRequestValidatorTests
{
    private readonly ListTodoItemsRequestValidator _validator = new();
    
    [Fact]
    public void ShouldNotHaveErrorsForValidRequest()
    {
        var request = Given.ListTodoItemsRequest();

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)] // Note: Minimum value, if supplied is 1
    [InlineData(51)] // Note: Maximum value, if supplied is 50
    public void LimitIsInvalid(int? value)
    {
        var request = new ListTodoItemsRequest
        {
            Limit = value
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Limit);
    }
    
    [Theory]
    [InlineData(null)] 
    [InlineData(1)] 
    [InlineData(50)] 
    public void LimitIsValid(int? value)
    {
        var request = new ListTodoItemsRequest
        {
            Limit = value
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Limit);
    }
}