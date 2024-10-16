using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit.Abstractions;

namespace Todo.EndToEndTests;

public class ListTodoItemsTests(Fixture fixture, ITestOutputHelper output) : TestClass<Fixture>(fixture, output)
{
    [Fact]
    public async Task ListTodoItemsBasic_Ok()
    {
        var tenantId = Given.TenantId();
        var client = Fixture.Client;
        
        int total = 10;
        
        {
            for (var i = 0; i < total; i++)
            {
                var args = Given.CreateTodoItemArgs(tenantId);
                await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
            }
        }

        {
            var args = Given.CreateTodoItemArgs(); // Note: Different Tenant Id
            await Fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        }
        
        Func<Task> asyncRetry = async () =>
        {
            // Note: The default API behaviour is to return 25 records
            var response = await client.V1.Tenant[tenantId].Todo.GetAsync();
            
            response.Should().NotBeNull();
            response!.TodoItems.Should().NotBeNull();
            
            response.TodoItems!.Count.Should().Be(total);
            response.TodoItems.Should().OnlyHaveUniqueItems();
        };
        
        await asyncRetry.Should().NotThrowAfterAsync(waitTime: 5.Seconds(), pollInterval: 1.Seconds());
    }
}