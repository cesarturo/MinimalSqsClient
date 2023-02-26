using MinimalSqsClient.HttpHandlers;

namespace MinimalSqsClient.Internal;

internal static class HttpHandlersConfigurator
{
    private static DelegatingHandler ChainHttpHandlers(IList<DelegatingHandler> handlers)
    {
        var outerHandler = handlers[0];
        var previous = outerHandler;
        for (int i = 1; i < handlers.Count; i++)
        {
            previous.InnerHandler = handlers[i];
        }
        handlers[^1].InnerHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };
        return outerHandler;
    }

    public static DelegatingHandler GetConfiguredHttpHandler(SqsClientOptions options)
    {
        var handlers = GetDefaultHandlersList(options);
        options.ConfigureHttpHandlers?.Invoke(handlers);
        return ChainHttpHandlers(handlers);
    }

    private static IList<DelegatingHandler> GetDefaultHandlersList(SqsClientOptions options)
    {
        var region = options.Region ?? QueueUrlRegionExtractor.Extract(options.QueueUrl);
        var signerHandler = new AwsSignatureV4SignerHttpHandler(region, options.Credentials);
        return new List<DelegatingHandler> { signerHandler };
    }
}