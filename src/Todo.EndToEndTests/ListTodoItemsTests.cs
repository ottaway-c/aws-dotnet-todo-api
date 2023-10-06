using FluentAssertions;
using FluentAssertions.Extensions;

namespace Todo.EndToEndTests;

public class ListTodoItemsTests
{
    [Fact]
    public async Task ListTodoItemsBasicOk()
    {
        var fixture = await Fixture.Ensure();
        var client = fixture.Client;
        
        var tenantId = Ulid.NewUlid();
        
        int total = 10;
        
        {
            for (var i = 0; i < total; i++)
            {
                var args = Given.CreateTodoItemArgs(tenantId);
                await fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
            }
        }

        {
            var args = Given.CreateTodoItemArgs(); // Note: Different Tenant Id
            await fixture.DdbStore.CreateTodoItemAsync(args, CancellationToken.None);
        }
        
        Func<Task> asyncRetry = async () =>
        {
            // Note: The default API behaviour is to return 25 records
            var response = await client.ListTodoItemsAsync(tenantId);
            
            response.Should().NotBeNull();
            response!.TodoItems.Should().NotBeNull();
            
            response.TodoItems.Count.Should().Be(total);
            response.TodoItems.Should().OnlyHaveUniqueItems();
        };
        
        await asyncRetry.Should().NotThrowAfterAsync(waitTime: 5.Seconds(), pollInterval: 1.Seconds());
    }
}