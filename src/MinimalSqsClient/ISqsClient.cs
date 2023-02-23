
namespace MinimalSqsClient;
public interface ISqsClient
{
    string Name { get; }
    Task<List<SqsMessage>> ReceiveMessages(int maxNumberOfMessages=1, int waitTimeSeconds=5, int visibilityTimeout = 30);
    Task<SqsMessage?> ReceiveMessage(int waitTimeSeconds = 5, int visibilityTimeout = 30);
    Task DeleteMessage(string receiptHandle);
    Task ChangeMessageVisibility(string receiptHandle, int visibilityTimeout);
    Task<bool[]> ChangeMessageVisibilityBatch(string[] receiptHandles, int visibilityTimeout);
    Task<string> SendMessage(string body, IDictionary<string, string> messageAttributes, int? delaySeconds = null);
}