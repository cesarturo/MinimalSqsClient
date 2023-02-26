﻿using Amazon.Runtime;

namespace MinimalSqsClient;

public sealed class SqsClientOptions
{
    public string? Region { get; set; }
    public string QueueUrl { get; set; }
    public AWSCredentials? Credentials { get; set; }

    public Action<IList<DelegatingHandler>>? ConfigureHttpHandlers { get; }
    public Action<HttpClient>? ConfigureHttpClient { get; set; }
}