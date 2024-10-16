using System.Collections.Concurrent;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Xunit.Abstractions;
using Xunit.Priority;

namespace Todo.IntegrationTests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public abstract class TestBase<TAppFixture> : IAsyncLifetime, IClassFixture<TAppFixture>
    where TAppFixture : class
{
    protected Fixture Fixture { get; set; }
    protected ITestOutputHelper Output { get; set; }

    protected TestBase(Fixture fixture, ITestOutputHelper output)
    {
        Fixture = fixture;
        Output = output;

        Log.Logger = new LoggerConfiguration().MinimumLevel.Information().Enrich.FromLogContext().WriteTo.TestOutput(output).CreateLogger();
    }

    protected virtual Task SetupAsync() => Task.CompletedTask;
    protected virtual Task TearDownAsync() => Task.CompletedTask;

    Task IAsyncLifetime.InitializeAsync() => SetupAsync();

    Task IAsyncLifetime.DisposeAsync() => TearDownAsync();
}

public abstract class AppFixture<TProgram> : IAsyncLifetime
    where TProgram : class
{
    protected static readonly ConcurrentDictionary<Type, object> WafCache = new();

    public IServiceProvider Services => _app.Services;

    public TestServer Server => _app.Server;

    public HttpClient Client { get; set; } = null!;

    WebApplicationFactory<TProgram> _app = null!;

    readonly IMessageSink? _messageSink;
    readonly ITestOutputHelper? _outputHelper;

    protected AppFixture(IMessageSink s)
    {
        _messageSink = s;
    }

    protected AppFixture(ITestOutputHelper h)
    {
        _outputHelper = h;
    }

    protected AppFixture(IMessageSink s, ITestOutputHelper h)
    {
        _messageSink = s;
        _outputHelper = h;
    }

    protected AppFixture() { }

    protected virtual Task PreSetupAsync() => Task.CompletedTask;

    protected virtual Task SetupAsync() => Task.CompletedTask;

    protected virtual Task TearDownAsync() => Task.CompletedTask;

    protected virtual void ConfigureApp(IWebHostBuilder a) { }

    protected virtual void ConfigureServices(IServiceCollection s) { }

    public HttpClient CreateClient(WebApplicationFactoryClientOptions? o = null) => CreateClient(_ => { }, o);

    public HttpClient CreateClient(Action<HttpClient> c, WebApplicationFactoryClientOptions? o = null)
    {
        var client = o is null ? _app.CreateClient() : _app.CreateClient(o);
        c(client);

        return client;
    }

    public HttpMessageHandler CreateHandler(Action<HttpContext>? c = null) => c is null ? _app.Server.CreateHandler() : _app.Server.CreateHandler(c);

    async Task IAsyncLifetime.InitializeAsync()
    {
        await PreSetupAsync();

        var type = GetType();

        _app = (WebApplicationFactory<TProgram>)WafCache.GetOrAdd(type, WafInitializer);
        
        Client = _app.CreateClient();

        await SetupAsync();

        object WafInitializer(Type _) =>
            new WebApplicationFactory<TProgram>().WithWebHostBuilder(b =>
            {
                b.UseEnvironment("Testing");
                b.ConfigureTestServices(ConfigureServices);
                ConfigureApp(b);
            });
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await TearDownAsync();
        Client.Dispose();
    }
}
