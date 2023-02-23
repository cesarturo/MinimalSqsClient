namespace MinimalSqsClient;

public interface ISqsClientFactory
{
    ISqsClient Get(string? name = null);
}