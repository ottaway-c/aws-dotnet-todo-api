using Microsoft.AspNetCore.Http.HttpResults;

namespace Todo.Api.Endpoints;

public class PingEndpointResponse
{
    public bool Ok { get; set; }
}

public class PingEndpoint : EndpointWithoutRequest<Ok<PingEndpointResponse>>
{
    public override void Configure()
    {
        Get("", "ping");
    }

    public override Task<Ok<PingEndpointResponse>> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(TypedResults.Ok(new PingEndpointResponse { Ok = true }));
    }
}
