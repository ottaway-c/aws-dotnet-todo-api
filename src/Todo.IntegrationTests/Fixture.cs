using Microsoft.Extensions.DependencyInjection;
using Todo.Api;
using Todo.Core;
using Xunit.Abstractions;
using Mapper = Todo.Api.Mapper;

namespace Todo.IntegrationTests;

public class Fixture(IMessageSink sink) : AppFixture<Program>(sink)
{
    public IDynamoDbStore DdbStore => Services.GetRequiredService<IDynamoDbStore>();
    public Mapper Mapper => Services.GetRequiredService<Mapper>();
}
