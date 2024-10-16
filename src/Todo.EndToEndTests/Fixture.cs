using System.Text;
using Amazon;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using dotenv.net;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using Todo.Client;
using Todo.Core;

namespace Todo.EndToEndTests;

public class Fixture
{
    public required IDynamoDbStore DdbStore { get; init; }
    public required ITodoClient Client { get; init; }

    public static async Task<Fixture> Ensure() => await _factory;

    private static readonly AsyncLazy<Fixture> _factory = new(async () =>
    {
        DotEnv.Fluent()
            .WithTrimValues()
            .WithEncoding(Encoding.UTF8)
            .WithOverwriteExistingVars()
            .WithProbeForEnv(8)
            .Load();
        
        var service = Env.GetString("SERVICE");
        var stage = Env.GetString("STAGE");
        var region = Env.GetString("CDK_DEFAULT_REGION");

        var chain = new CredentialProfileStoreChain();
        IAmazonAPIGateway apiGateway;
        
        if (chain.TryGetAWSCredentials("todo-api", out var credentials))
        {
            // Running locally
            apiGateway = new AmazonAPIGatewayClient(credentials, RegionEndpoint.GetBySystemName(region));
        }
        else
        {
            // Running in Github actions
            credentials = FallbackCredentialsFactory.GetCredentials();
            apiGateway = new AmazonAPIGatewayClient();
        }

        var provider = new AWSCredentialsProvider(credentials);
        
        var tableNamePrefix = $"{service}-{stage}-app-";
        var regionEndpoint = EfficientDynamoDb.Configs.RegionEndpoint.Create(region);
        var config = new DynamoDbContextConfig(regionEndpoint, provider)
        {
            TableNamePrefix = tableNamePrefix
        };
        
        var ddb = new DynamoDbContext(config);
        var ddbStore = new DynamoDbStore(ddb);
        
        // Note:
        // Perform service discovery to find our deployed API URL
        var apiGatewayUrl = await apiGateway.GetApiGatewayUrlAsync(service, stage, region);
        var client = new TodoClient(new HttpClient(), apiGatewayUrl);

        return new Fixture
        {
            DdbStore = ddbStore,
            Client = client
        };
    });
}

public static class ApiGatewayExtensions
{
    public static async Task<Uri> GetApiGatewayUrlAsync(this IAmazonAPIGateway apiGateway, string service, string stage, string region)
    {
        var apis = await apiGateway.GetRestApisAsync(new GetRestApisRequest { Limit = 25 });
        var apiName = $"{service}-{stage}-app";
        var api = apis.Items.FirstOrDefault(x => x.Name.Equals(apiName, StringComparison.OrdinalIgnoreCase));
        
        if (api == null) throw new Exception($"Could not find Api Gateway {apiName}");
        
        var apiGatewayUrl = new Uri($"https://{api.Id}.execute-api.{region}.amazonaws.com/LIVE/");
        return apiGatewayUrl;
    }
}