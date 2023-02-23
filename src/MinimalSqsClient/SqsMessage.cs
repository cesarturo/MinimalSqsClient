namespace MinimalSqsClient;

public sealed class SqsMessage
{
    public string MessageId { get; set; }
    public string ReceiptHandle { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> MessageAttributes { get; set; }
}