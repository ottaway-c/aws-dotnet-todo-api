using System.Text;
using dotenv.net;
using EfficientDynamoDb;
using EfficientDynamoDb.Credentials.AWSSDK;
using FastEndpoints.ClientGen.Kiota;
using FastEndpoints.Swagger;
using Kiota.Builder;
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
        builder.Services.AddExceptionHandler<ExceptionResponseHandler>();
        builder.Services.AddFastEndpoints();
        builder.Services.SwaggerDocument(x =>
        {
            x.ShortSchemaNames = true;
            x.MaxEndpointVersion = 1;
            x.DocumentSettings = s =>
            {
                s.Title = "Todo API";
                s.DocumentName = "v1";
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
            x.Versioning.PrependToRoute = true;
            x.Endpoints.ShortNames = true;
            x.Endpoints.Configurator = ep =>
            {
                ep.DontCatchExceptions();
                ep.PreProcessor<TenantIdChecker>(Order.Before);
                
                // Note: Obviously in a production scenario this API would be secured
                // I've made it anonymous to keep things simple
                ep.AllowAnonymous();

                foreach (var route in ep.Routes)
                {
                    if (route is not ("" or "ping"))
                    {
                        ep.Description(b => b.Produces<ApiErrorResponse>(500));
                    }
                }
            };
        });
        
        app.UseSwaggerGen();
        
        await app.GenerateApiClientsAndExitAsync(cs =>
        {
            cs.Language = GenerationLanguage.CSharp;
            cs.SwaggerDocumentName = "v1";
            cs.OutputPath = "../Todo.Client";
            cs.ClientNamespaceName = "Todo.Client";
            cs.ClientClassName = "TodoClient";
        });
        
        await app.RunAsync();
    }
}