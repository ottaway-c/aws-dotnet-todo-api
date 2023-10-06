using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using FastEndpoints.Testing;
using Todo.Api;
using Todo.Core;
using Xunit.Abstractions;
using Mapper = Todo.Api.Mapper;

namespace Todo.IntegrationTests;

public class Fixture : TestFixture<Program>
{
    public required IDynamoDbStore DdbStore { get; init; }
    public required Mapper Mapper { get; init; }
    
    public Fixture(IMessageSink sink) : base(sink)
    {
        var service = Env.GetString("SERVICE");
        var stage = Env.GetString("STAGE");
        var region = Env.GetString("CDK_DEFAULT_REGION");

        var chain = new CredentialProfileStoreChain();
        
        if (chain.TryGetAWSCredentials("clearpoint-api", out var credentials))
        {
            // Running locally
        }
        else
        {
            // Running in Github actions
            credentials = FallbackCredentialsFactory.GetCredentials();
        }
        
        var provider = new AWSCredentialsProvider(credentials);
        
        var tableNamePrefix = $"{service}-{stage}-app-";
        var regionEndpoint = EfficientDynamoDb.Configs.RegionEndpoint.Create(region);
        var config = new DynamoDbContextConfig(regionEndpoint, provider)
        {
            TableNamePrefix = tableNamePrefix
        };
        
        var ddb = new DynamoDbContext(config);
        DdbStore = new DynamoDbStore(ddb);

        Mapper = new Mapper();
    }
}