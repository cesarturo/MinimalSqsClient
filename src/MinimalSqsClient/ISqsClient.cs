
namespace MinimalSqsClient;
public interface ISqsClient
{
    string Name { get; }
    Task<List<SqsMessage>> ReceiveMessagesAsync(int maxNumberOfMessages=1, int waitTimeSeconds=5, int visibilityTimeout = 30);
    Task<SqsMessage?> ReceiveMessageAsync(int waitTimeSeconds = 5, int visibilityTimeout = 30);
    Task DeleteMessageAsync(string receiptHandle);
    Task ChangeMessageVisibilityAsync(string receiptHandle, int visibilityTimeout);
    Task<bool[]> ChangeMessageVisibilityBatchAsync(string[] receiptHandles, int visibilityTimeout);
    Task<string> SendMessageAsync(string body, IDictionary<string, string> messageAttributes, int? delaySeconds = null);
    Task<string[]> SendMessageBatchAsync(string[] bodies, IDictionary<string, string>? messageAttributes = null, int? delaySeconds = null);
    Task<bool> PurgeQueueAsync();
}