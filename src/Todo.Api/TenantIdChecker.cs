namespace Todo.Api;

public interface ITenantId
{
    public string? TenantId { get; set; }
}

public class TenantIdChecker : IGlobalPreProcessor
{
    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken cancellationToken)
    {
        if (context.Request is ITenantId request)
        {
            // NOTE:
            // Standardise the tenant id
            // This is important as we store the tenant id in Dynamo as part of the PK
            // Casing inconsistency could cause queries/updates to fail
            request.TenantId = request.TenantId!.ToLower();

            await Task.CompletedTask;
        }
    }
}
