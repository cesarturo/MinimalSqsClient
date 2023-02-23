namespace MinimalSqsClient;

internal static class QueueUrlRegionExtractor
{
    public static string Extract(string? queueUrl, string? name=null)
    {
        if (queueUrl is null) throw Exceptions.QueueUrlNullException(name);

        int regionStart = queueUrl.IndexOf('.') + 1;
        if (regionStart is 0) throw Exceptions.CannotInferRegionException(name);

        int regionEnd = queueUrl.IndexOf(".amazonaws");
        if (regionEnd < 0) throw Exceptions.CannotInferRegionException(name);

        return queueUrl.Substring(regionStart, regionEnd - regionStart);
    }

    
}