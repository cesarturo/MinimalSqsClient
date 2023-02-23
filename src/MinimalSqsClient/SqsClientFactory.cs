using Microsoft.Extensions.Options;

namespace MinimalSqsClient;

public class SqsClientFactory: ISqsClientFactory
{
    private readonly Dictionary<string, ISqsClient> _sqsClients;
    public SqsClientFactory(IEnumerable<ISqsClient> sqsClients)
    {
        _sqsClients = sqsClients.ToDictionary(c => c.Name, c => c);
    }
    public ISqsClient Get(string? name = null)
    {
        name ??= Options.DefaultName;
        return _sqsClients.TryGetValue(name, out var sqsClient)
            ? sqsClient
            : throw new Exception($"SqsClient with name {name} is not registered");
    }
}