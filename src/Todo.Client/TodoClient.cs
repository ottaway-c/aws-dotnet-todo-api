using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace Todo.Client;

public interface ITodoClient
{
    Task<CreateTodoItemResponse> CreateTodoItemAsync(CreateTodoItemRequest request);

    Task<UpdateTodoItemResponse> UpdateTodoItemAsync(Ulid tenantId, Ulid todoItemId, UpdateTodoItemRequest request);

    Task<bool> DeleteTodoItemAsync(Ulid tenantId, Ulid todoItemId);

    Task<GetTodoItemResponse?> GetTodoItemAsync(Ulid tenantId, Ulid todoItemId);

    Task<ListTodoItemsResponse> ListTodoItemsAsync(Ulid tenantId, int? limit = null, string? paginationToken = null, bool? isCompleted = null);
}

public class TodoClient : ITodoClient
{
    private readonly HttpClient _httpClient;
    
    private static AsyncRetryPolicy RetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 5);

        return Policy.Handle<TodoClientException>(exception =>
            {
                return (int)exception.StatusCode >= 500 || exception.StatusCode switch
                {
                    HttpStatusCode.RequestTimeout => true,
                    HttpStatusCode.GatewayTimeout => true,
                    HttpStatusCode.NotFound => true,        // NOTE: Retry not found, for eventual consistency reasons.
                    HttpStatusCode.BadGateway => true,
                    _ => false
                };
            })
            .WaitAndRetryAsync(delay);
    }

    public TodoClient(HttpClient httpClient, Uri serviceUrl)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = serviceUrl;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }
    
    public async Task<CreateTodoItemResponse> CreateTodoItemAsync(CreateTodoItemRequest request)
    {
        return await RetryPolicy().ExecuteAsync(async () =>
        {
            var url = new Uri($"v1/api/{request.TenantId}/todo", UriKind.Relative);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            using var httpResponse = await _httpClient.SendAsync(httpRequest);
            var content = await httpResponse.Content.ReadAsStringAsync();

            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    var result = JsonSerializer.Deserialize<CreateTodoItemResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })!;

                    return result;
                }
                default:
                    throw new TodoClientException(httpResponse.StatusCode, content);
            }
        });
    }

    public async Task<UpdateTodoItemResponse> UpdateTodoItemAsync(Ulid tenantId, Ulid todoItemId, UpdateTodoItemRequest request)
    {
        return await RetryPolicy().ExecuteAsync(async () =>
        {
            var url = new Uri($"v1/api/{tenantId}/todo/{todoItemId}", UriKind.Relative);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Put, url);
            httpRequest.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            using var httpResponse = await _httpClient.SendAsync(httpRequest);
            var content = await httpResponse.Content.ReadAsStringAsync();

            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    var result = JsonSerializer.Deserialize<UpdateTodoItemResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })!;

                    return result;
                }
                default:
                    throw new TodoClientException(httpResponse.StatusCode, content);
            }
        });
    }

    public async Task<bool> DeleteTodoItemAsync(Ulid tenantId, Ulid todoItemId)
    {
        return await RetryPolicy().ExecuteAsync(async () =>
        {
            var url = new Uri($"v1/api/{tenantId}/todo/{todoItemId}", UriKind.Relative);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, url);

            using var httpResponse = await _httpClient.SendAsync(httpRequest);
            var content = await httpResponse.Content.ReadAsStringAsync();

            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.NoContent:
                {
                    return true;
                }
                case HttpStatusCode.NotFound:
                {
                    return false;
                }
                default:
                    throw new TodoClientException(httpResponse.StatusCode, content);
            }
        });
    }

    public async Task<GetTodoItemResponse?> GetTodoItemAsync(Ulid tenantId, Ulid todoItemId)
    {
        return await RetryPolicy().ExecuteAsync(async () =>
        {
            var url = new Uri($"v1/api/{tenantId}/todo/{todoItemId}", UriKind.Relative);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);

            using var httpResponse = await _httpClient.SendAsync(httpRequest);
            var content = await httpResponse.Content.ReadAsStringAsync();

            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    var result = JsonSerializer.Deserialize<GetTodoItemResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })!;

                    return result;
                }
                case HttpStatusCode.NotFound:
                    return null;
                default:
                    throw new TodoClientException(httpResponse.StatusCode, content);
            }
        });
    }

    public async Task<ListTodoItemsResponse> ListTodoItemsAsync(Ulid tenantId, int? limit = null, string? paginationToken = null, bool? isCompleted = null)
    {
        return await RetryPolicy().ExecuteAsync(async () =>
        {
            var url = $"v1/api/{tenantId}/todo/?limit={limit ?? 25}";
            if (paginationToken != null) url += $"&paginationToken={paginationToken}";
            if (isCompleted != null) url += $"&isCompleted={isCompleted}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, url);
            
            using var httpResponse = await _httpClient.SendAsync(httpRequest);
            var content = await httpResponse.Content.ReadAsStringAsync();

            switch (httpResponse.StatusCode)
            {
                case HttpStatusCode.OK:
                {
                    var result = JsonSerializer.Deserialize<ListTodoItemsResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    })!;

                    return result;
                }
                default:
                    throw new TodoClientException(httpResponse.StatusCode, content);
            }
        });
    }
}