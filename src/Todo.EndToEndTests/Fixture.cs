using System.Text;
using Amazon;
using Amazon.APIGateway;
using Amazon.APIGateway.Model;
using dotenv.net;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using Microsoft.Kiota.Abstractions.Authentication;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Todo.Client;
using Todo.Core;

namespace Todo.EndToEndTests;

public class Fixture : TestFixture
{
    public required IDynamoDbStore DdbStore { get; set; }
    public required TodoClient Client { get; set; }

    protected override async Task SetupAsync()
    {
        DotEnv.Fluent().WithTrimValues().WithEncoding(Encoding.UTF8).WithOverwriteExistingVars().WithProbeForEnv(8).Load();
        
        var credentials = Env.GetAwsCredentials("todo-api");
        var region = Env.GetRegion();
        var regionEndpoint = RegionEndpoint.GetBySystemName(region);
        
        var service = Env.GetString("SERVICE");
        var stage = Env.GetString("STAGE");
        
        var provider = new AWSCredentialsProvider(credentials);
        
        var tableNamePrefix = $"{service}-{stage}-app-";
        var config = new DynamoDbContextConfig(EfficientDynamoDb.Configs.RegionEndpoint.Create(region), provider)
        {
            TableNamePrefix = tableNamePrefix
        };
        
        var ddb = new DynamoDbContext(config);
        DdbStore = new DynamoDbStore(ddb);
        
        var apiGateway = new AmazonAPIGatewayClient(credentials, regionEndpoint);
        
        // Note:
        // Perform service discovery to find our deployed API URL
        var apiGatewayUrl = await apiGateway.GetApiGatewayUrlAsync(service, stage, region);
        
        var adapter = new HttpClientRequestAdapter(new AnonymousAuthenticationProvider()) { BaseUrl = apiGatewayUrl.ToString() };
        Client = new TodoClient(adapter);
    }
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