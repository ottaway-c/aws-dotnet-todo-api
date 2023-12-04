using System.Text;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using dotenv.net;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using FastEndpoints.Swagger;
using Serilog;
using Todo.Core;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

DotEnv.Fluent()
    .WithTrimValues()
    .WithEncoding(Encoding.UTF8)
    .WithOverwriteExistingVars()
    .WithProbeForEnv(8)
    .Load();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

//
// Hosting
//

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);
builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(x =>
{
    x.MaxEndpointVersion = 1;
    x.DocumentSettings = s =>
    {
        s.Title = "Todo API";
        s.DocumentName = "Initial Release";
        s.Version = "v1";
    };
});

//
// Dynamo Db
//

builder.Services.AddSingleton(x =>
{
    var service = Env.GetString("SERVICE");
    var stage = Env.GetString("STAGE");
    var region = Env.GetString("AWS_REGION");
   
    var regionEndpoint = EfficientDynamoDb.Configs.RegionEndpoint.Create(region);
    var chain = new CredentialProfileStoreChain();
    
    if (chain.TryGetAWSCredentials("todo-api", out var credentials))
    {
        // Running locally
    }
    else
    {
        // Running in AWS/Docker/Github actions
        credentials = FallbackCredentialsFactory.GetCredentials();
    }
    
    var provider = new AWSCredentialsProvider(credentials);
    
    var tableNamePrefix = $"{service}-{stage}-app-";
    var config = new DynamoDbContextConfig(regionEndpoint, provider)
    {
        TableNamePrefix = tableNamePrefix
    };
    
    var ddb = new DynamoDbContext(config);
    return (IDynamoDbContext)ddb;
});
builder.Services.AddSingleton<IDynamoDbStore, DynamoDbStore>();

//
// Application
//

var app = builder.Build();

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseDefaultExceptionHandler();
app.UseFastEndpoints(x =>
{
    x.Versioning.Prefix = "v";
    x.Versioning.DefaultVersion = 1;
    x.Versioning.PrependToRoute = true;

    // Note: Obviously in a production scenario this API would be secured
    // I've made it anonymous to keep things simple
    x.Endpoints.Configurator = epd => epd.AllowAnonymous();
});
app.UseSwaggerGen();

app.Run();

namespace Todo.Api
{
    public partial class Program { }
}