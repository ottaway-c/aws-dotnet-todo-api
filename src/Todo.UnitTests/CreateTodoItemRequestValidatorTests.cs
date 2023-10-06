using FluentValidation.TestHelper;
using Todo.Api.Endpoints;

namespace Todo.UnitTests;

public class CreateTodoItemRequestValidatorTests
{
    private readonly CreateTodoItemRequestValidator _validator = new();
    
    [Fact]
    public void ShouldNotHaveErrorsForValidRequest()
    {
        var request = Given.CreateTodoItemRequest();

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void TenantIdIsRequired()
    {
        var request = new CreateTodoItemRequest
        {
            TenantId = null,
        };
        
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TenantId);
    }

    [Theory]
    [ClassData(typeof(Given.TitleTestData))]
    public void TitleIsRequired(string? value)
    {
        var request = new CreateTodoItemRequest
        {
            Title = value,
        };
        
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }
    
    [Theory]
    [ClassData(typeof(Given.NotesTestData))]
    public void NotesAreRequired(string? value)
    {
        var request = new CreateTodoItemRequest
        {
            Notes = value,
        };
        
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }
}



