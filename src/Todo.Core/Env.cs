using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;

namespace Todo.Core;

public static class Env
{
    public static string GetString(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new Exception($"Env var {key} was missing");
        return value;
    }

    public static string GetRegion()
    {
        var value = Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION");
        return string.IsNullOrWhiteSpace(value) ? GetString("AWS_REGION") : value;
    }

    public static AWSCredentials GetAwsCredentials(string profile)
    {
        var chain = new CredentialProfileStoreChain();
        if (chain.TryGetAWSCredentials(profile, out var credentials))
        {
            // Running locally
        }
        else
        {
            // Running in AWS/Docker/Github actions
            credentials = FallbackCredentialsFactory.GetCredentials();
        }
        return credentials;
    }
}
