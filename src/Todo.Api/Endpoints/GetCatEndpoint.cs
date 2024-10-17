using Microsoft.AspNetCore.Http.HttpResults;

namespace Todo.Api.Endpoints;

public class GetCatRequest : ITenantId
{
    public string? TenantId { get; set; }
}

public class GetCatResponse
{
   public required string Meow { get; init; }
}

public class GetCatRequestValidator : Validator<GetCatRequest>
{
    public GetCatRequestValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
    }
}

public class GetCatEndpoint : Endpoint<GetCatRequest, Ok<GetCatResponse>>
{
    private const string Cat = @"""
   |\__/,|   (`\
   |o o  |__ _)
 _.( T   )  `  /
((_ `^--' /_<  \
`` `-'(((/  (((/
""";
    
    public override void Configure()
    {
        Get("tenant/{TenantId}/cat");
        Version(1);
        Summary(s =>
        {
            s.Summary = "Get a Cat";
            s.Description = "Returns a friendly cat";
        });
        Validator<GetCatRequestValidator>();
    }

    public override async Task<Ok<GetCatResponse>> ExecuteAsync(GetCatRequest request, CancellationToken ct)
    {
        await Task.CompletedTask;
        return TypedResults.Ok(new GetCatResponse
        {
            Meow = Cat
        });
    }
}