using System.Net;

namespace Todo.Client;

public class TodoClientException : Exception
{
    public HttpStatusCode StatusCode { get; }

    public string Content { get; }
        
    public TodoClientException(HttpStatusCode statusCode, string content) : base(FormatMessage(statusCode, content))
    {
        StatusCode = statusCode;
        Content = content;
    }

    private static string FormatMessage(HttpStatusCode statusCode, string content)
    {
        return $"StatusCode: {statusCode} Response:\n{content}";
    }
}