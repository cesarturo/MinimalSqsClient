using Amazon.Runtime;
using Microsoft.Extensions.Options;

namespace MinimalSqsClient;

public sealed class SqsClient : ISqsClient, IDisposable
{
    public string Name { get; }
    private readonly HttpClient _httpClient;

    public SqsClient(SqsClientOptions options, string? name = null)
    {
        Name = name ?? Options.DefaultName;
        var region = options.Region ?? QueueUrlRegionExtractor.Extract(options.QueueUrl);
        _httpClient = CreateHttpClient(options.QueueUrl, options.Credentials, region);
    }

    private HttpClient CreateHttpClient(string queueUrl, AWSCredentials? credentials, string region)
    {
        var handler = new RequestSignerHttpHandler(region, credentials);
        handler.InnerHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };
        return new HttpClient(handler)
        {
            BaseAddress = new Uri(queueUrl)
        };
    }

    public async Task<List<SqsMessage>> ReceiveMessagesAsync(int maxNumberOfMessages = 1, int waitTimeSeconds = 5, int visibilityTimeout = 30)
    {
        var parameters = new KeyValuePair<string, string>[]
        {
            new("Action", "ReceiveMessage"),
            new("MaxNumberOfMessages", maxNumberOfMessages.ToString()),
            new("MessageAttributeName.1", "All"),
            new("Version", "2012-11-05"),
            new("VisibilityTimeout", visibilityTimeout.ToString()),
            new("WaitTimeSeconds", waitTimeSeconds.ToString())
        };
        var response = await _httpClient.PostAsync("", new FormUrlEncodedContent(parameters));

        response.EnsureSuccessStatusCode();

        return ReceiveMessageResponseReader.ReadSqsMessages(response);
    }
    public async Task<SqsMessage?> ReceiveMessageAsync(int waitTimeSeconds = 5, int visibilityTimeout = 30)
    {
        var parameters = new KeyValuePair<string, string>[]
        {
            new("Action", "ReceiveMessage"),
            new("MaxNumberOfMessages", "1"),
            new("MessageAttributeName.1", "All"),
            new("Version", "2012-11-05"),
            new("VisibilityTimeout", visibilityTimeout.ToString()),
            new("WaitTimeSeconds", waitTimeSeconds.ToString())
        };
        var response = await _httpClient.PostAsync("", new FormUrlEncodedContent(parameters));

        response.EnsureSuccessStatusCode();

        return ReceiveMessageResponseReader.ReadSqsMessage(response);
    }

    public async Task DeleteMessageAsync(string receiptHandle)
    {
        var parameters = new KeyValuePair<string, string>[]
        {
            new("Action", "DeleteMessage"),
            new("ReceiptHandle", receiptHandle),
            new("Version", "2012-11-05")
        };
        var response = await _httpClient.PostAsync("", new FormUrlEncodedContent(parameters));

        response.EnsureSuccessStatusCode();
    }

    public async Task ChangeMessageVisibilityAsync(string receiptHandle,  int visibilityTimeout)
    {
        var parameters = new KeyValuePair<string, string>[]
        {
            new("Action", "ChangeMessageVisibility"),
            new("ReceiptHandle", receiptHandle),
            new("VisibilityTimeout", visibilityTimeout.ToString()),
            new("Version", "2012-11-05")
        };
        var response = await _httpClient.PostAsync("", new FormUrlEncodedContent(parameters));

        response.EnsureSuccessStatusCode();
    }
    public async Task<bool[]> ChangeMessageVisibilityBatchAsync(string[] receiptHandles, int visibilityTimeout)
    {
        var parameters = new List<KeyValuePair<string, string>>(2+ 3* receiptHandles.Length)
        {
            new("Action", "ChangeMessageVisibilityBatch"),
            new("Version", "2012-11-05")
        };
        for (int i = 0; i < receiptHandles.Length; i++)
        {
            parameters.Add(new($"ChangeMessageVisibilityBatchRequestEntry.{i + 1}.Id", i.ToString()));
            parameters.Add(new($"ChangeMessageVisibilityBatchRequestEntry.{i + 1}.ReceiptHandle", receiptHandles[i]));
            parameters.Add(new($"ChangeMessageVisibilityBatchRequestEntry.{i + 1}.VisibilityTimeout", visibilityTimeout.ToString()));
        }

        var httpResponse = await _httpClient.PostAsync("", new FormUrlEncodedContent(parameters));

        httpResponse.EnsureSuccessStatusCode();
        var listOfIdAndSuccess = ChangeMessageVisibilityBatchResponseReader.Read(httpResponse.EnsureSuccessStatusCode(), receiptHandles.Length);
        var result = new bool[receiptHandles.Length];
        for (int i = 0; i < listOfIdAndSuccess.Count; i++)
        {
            result[listOfIdAndSuccess[i].id] = listOfIdAndSuccess[i].success;
        }
        return result;
    }

    public async Task<string> SendMessageAsync(string body, IDictionary<string, string>? messageAttributes = null, int? delaySeconds = null)
    {
        var attributeParametersCount = messageAttributes is null ? 0 : 3 * messageAttributes.Count;
        var parameters = new List<KeyValuePair<string, string>>(3 + attributeParametersCount + (delaySeconds.HasValue ? 1 : 0))
        {
            new("Action", "SendMessage"),
            new("MessageBody", body),            
            new("Version", "2012-11-05")
        };
        if (delaySeconds.HasValue)
        {
            parameters.Add(new KeyValuePair<string, string>("DelaySeconds", delaySeconds.Value.ToString()));
        }
        if (messageAttributes is not null)
        {
            int i=0;
            foreach (var (key, value) in messageAttributes)
            {
                i++;
                parameters.Add(new KeyValuePair<string, string>($"MessageAttribute.{i}.Name", key));
                parameters.Add(new KeyValuePair<string, string>($"MessageAttribute.{i}.Value.StringValue", value));
                parameters.Add(new KeyValuePair<string, string>($"MessageAttribute.{i}.Value.DataType", "String"));
            }
        }
        
        var response = await _httpClient.PostAsync("", new FormUrlEncodedContent(parameters));

        response.EnsureSuccessStatusCode();

        return SendMessageResponseReader.ReadMessageId(response);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}