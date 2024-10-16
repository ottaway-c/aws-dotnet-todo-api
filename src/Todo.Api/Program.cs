using System.Text;
using dotenv.net;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using FastEndpoints.Swagger;
using Serilog;
using Todo.Core;

namespace Todo.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        
        DotEnv.Fluent()
            .WithTrimValues()
            .WithEncoding(Encoding.UTF8)
            .WithOverwriteExistingVars()
            .WithProbeForEnv(8)
            .Load();
        
        FluentValidationOptions.Configure();
        
        var region = Env.GetRegion();
        var credentials = Env.GetAwsCredentials("todo-api");
        
        var service = Env.GetString("SERVICE");
        var stage = Env.GetString("STAGE");
        
        var builder = WebApplication.CreateBuilder(args);
        builder.Host.UseSerilog();
        
        builder.Services.AddSingleton<IDynamoDbContext>(_ =>
        {
            var provider = new AWSCredentialsProvider(credentials);
            var endpoint = EfficientDynamoDb.Configs.RegionEndpoint.Create(region);
            var config = new DynamoDbContextConfig(endpoint, provider) { TableNamePrefix = $"{service}-{stage}-app-" };
            return new DynamoDbContext(config);
        });

        builder.Services.AddSingleton<IDynamoDbStore, DynamoDbStore>();
        builder.Services.AddSingleton<Mapper>();
        
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
        // Application
        //

        var app = builder.Build();
        app.UseSerilogRequestLogging();
        app.UseDefaultExceptionHandler();
        app.UseFastEndpoints(x =>
        {
            x.Versioning.Prefix = "v";
            x.Versioning.DefaultVersion = 1;
            x.Versioning.PrependToRoute = true;
            x.Endpoints.ShortNames = true;

            // Note: Obviously in a production scenario this API would be secured
            // I've made it anonymous to keep things simple
            x.Endpoints.Configurator = epd => epd.AllowAnonymous();
        });
        app.UseSwaggerGen();
        
        await app.RunAsync();
    }
}