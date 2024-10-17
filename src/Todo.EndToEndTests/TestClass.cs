using Xunit.Abstractions;
using Xunit.Priority;

namespace Todo.EndToEndTests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public abstract class TestClass<TFixture>(TFixture f, ITestOutputHelper o) : IClassFixture<TFixture>
    where TFixture : class
{
    protected TFixture Fixture { get; init; } = f;
    protected ITestOutputHelper Output { get; init; } = o;
}
