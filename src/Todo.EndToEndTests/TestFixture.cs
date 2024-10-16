using Xunit.Abstractions;

namespace Todo.EndToEndTests;

public abstract class TestFixture : IAsyncLifetime
{
    protected TestFixture(IMessageSink s)
    {
        Initialize(s);
    }

    protected TestFixture(ITestOutputHelper h)
    {
        Initialize(null, h);
    }

    protected TestFixture(IMessageSink s, ITestOutputHelper h)
    {
        Initialize(s, h);
    }

    protected TestFixture()
    {
        Initialize();
    }

    void Initialize(IMessageSink? s = null, ITestOutputHelper? h = null) { }

    /// <summary>
    /// override this method if you'd like to do some one-time setup for the test-class.
    /// it is run before any of the test-methods of the class is executed.
    /// </summary>
    protected virtual Task SetupAsync() => Task.CompletedTask;

    /// <summary>
    /// override this method if you'd like to do some one-time teardown for the test-class.
    /// it is run after all test-methods have executed.
    /// </summary>
    protected virtual Task TearDownAsync() => Task.CompletedTask;

    public Task InitializeAsync() => SetupAsync();

    public virtual Task DisposeAsync()
    {
        return TearDownAsync();
    }
}