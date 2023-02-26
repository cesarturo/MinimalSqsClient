namespace MinimalSqsClient.Internal;

internal static class Exceptions
{
    public static Exception QueueUrlNullException(string? name = null)
    {
        string nameVerbiage = $" for SqsClient {name}";
        return new($"QueueUrl is null{nameVerbiage}.");
    }

    public static Exception CannotInferRegionException(string? name = null)
    {
        string nameVerbiage = $" for SqsClient {name}";
        return new($"Cannot infer region from QueueUrl{nameVerbiage}. Provide a region.");
    }
}