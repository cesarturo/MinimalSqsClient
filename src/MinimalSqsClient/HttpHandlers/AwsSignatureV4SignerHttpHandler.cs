using Amazon.Runtime;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MinimalSqsClient.HttpHandlers
{
    public sealed class AwsSignatureV4SignerHttpHandler : DelegatingHandler
    {
        private readonly string _region;
        private readonly AWSCredentials _credentials;
        private const string _service = "sqs";

        public AwsSignatureV4SignerHttpHandler(string region, AWSCredentials? credentials = null)
        {
            _region = region;
            _credentials = credentials ?? FallbackCredentialsFactory.GetCredentials();
        }
        //Docs auth:
        //https://docs.aws.amazon.com/AWSSimpleQueueService/latest/SQSDeveloperGuide/sqs-api-request-authentication.html
        //https://docs.aws.amazon.com/general/latest/gr/create-signed-request.html#code-signing-examples
        //https://stackoverflow.com/questions/575440/url-encoding-using-c-sharp
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var immutableCredentials = await _credentials.GetCredentialsAsync();
            var date = DateTime.UtcNow;
            var dateTimeString = date.ToString("yyyyMMddTHHmmssZ");
            var dateString = date.ToString("yyyyMMdd");

            var contentType = request.Content.Headers.ContentType.ToString().ToLower();
            var bodyHash = GetHashHex(request.Content.ReadAsByteArrayAsync().Result);


            request.Headers.Host = request.RequestUri.Host;
            request.Headers.Add("x-amz-security-token", immutableCredentials.Token);
            request.Headers.Add("X-Amz-Date", dateTimeString);
            request.Headers.Add("X-Amz-Content-SHA256", bodyHash);


            var canonicalRequest = CreateCanonicalRequest(request, dateTimeString, bodyHash, contentType, immutableCredentials);
            var canonicalRequestHash = GetHashHex(Encoding.UTF8.GetBytes(canonicalRequest));

            var stringToSign = $"AWS4-HMAC-SHA256\n{dateTimeString}\n{dateString}/{_region}/{_service}/aws4_request\n{canonicalRequestHash}";

            var secret         = immutableCredentials.SecretKey;
            var dates          = date.ToString("yyyyMMdd");
            var kDate          = HMACSHA256.HashData(Encoding.UTF8.GetBytes("AWS4" + secret), Encoding.UTF8.GetBytes(dates));
            var kRegion        = HMACSHA256.HashData(kDate, Encoding.UTF8.GetBytes(_region));
            var kService       = HMACSHA256.HashData(kRegion, Encoding.UTF8.GetBytes(_service));
            var kSigning       = HMACSHA256.HashData(kService, Encoding.UTF8.GetBytes("aws4_request"));
            var signatureBytes = HMACSHA256.HashData(kSigning, Encoding.UTF8.GetBytes(stringToSign));

            var signature = GetHex(signatureBytes);

            var authorizationHeaderValue = $"Credential={immutableCredentials.AccessKey}/{dateString}/{_region}/{_service}/aws4_request," +
                                           $" SignedHeaders=content-type;host;x-amz-content-sha256;x-amz-date;x-amz-security-token," +
                                           $" Signature={signature}";

            request.Headers.Authorization = new AuthenticationHeaderValue("AWS4-HMAC-SHA256", authorizationHeaderValue);

            return await base.SendAsync(request, cancellationToken);
        }


        private static string CreateCanonicalRequest(HttpRequestMessage request, string date, string bodyHash, string contentType, ImmutableCredentials immutableCredentials)
        {
            // request.method.metho.Length + 1 + request.RequestUri.AbsolutePath.Length + 1
            // + request.RequestUri.Query.Length + 1
            // + 82 + contentType.Length + request.header.hoost.Length+ bodyHash.Length + date.Length + ImmutableCredentials.Token.Length
            // + 71
            // + bodyHash.Length
            var approximateLength = 156 + request.Method.Method.Length + request.RequestUri.AbsolutePath.Length
                                    + request.RequestUri.Query.Length + contentType.Length + request.Headers.Host.Length
                                    + 2 * bodyHash.Length + date.Length + immutableCredentials.Token.Length;
            var canonicalRequestBuilder = new StringBuilder(approximateLength)
            .Append(request.Method.Method).Append('\n')
            .Append(GetCanonicalAbsolutePath(request)).Append('\n');

            var queryStringValues = HttpUtility.ParseQueryString(request.RequestUri.Query); //this UrlDecodes key and value
            var orderedQueryStringValues = queryStringValues.AllKeys.OrderBy(key => key)
                .Select(key => (key, queryStringValues[key]));
            bool notFirst = false;
            foreach (var (key, value) in orderedQueryStringValues)
            {
                if (notFirst)
                {
                    canonicalRequestBuilder.Append('&');
                    notFirst = true;
                }
                canonicalRequestBuilder.Append(Uri.EscapeDataString(key)).Append('=').Append(Uri.EscapeDataString(value));
            }
            canonicalRequestBuilder.Append('\n')

            
            .Append("content-type:").Append(contentType).Append('\n')
            .Append("host:").Append(request.Headers.Host.Trim()).Append('\n')
            .Append("x-amz-content-sha256:").Append(bodyHash).Append('\n')
            .Append("x-amz-date:").Append(date).Append('\n')
            .Append("x-amz-security-token:").Append(immutableCredentials.Token).Append('\n')
            .Append('\n')

            .Append("content-type;host;x-amz-content-sha256;x-amz-date;x-amz-security-token").Append('\n')

            .Append(bodyHash);

            return canonicalRequestBuilder.ToString();
        }

        private static string GetCanonicalAbsolutePath(HttpRequestMessage request)
        {
            var absolutePath = request.RequestUri.AbsolutePath;
            return absolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString)
                .Aggregate(new StringBuilder(absolutePath.Length), (builder, segment) => builder.Append('/').Append(segment))
                .ToString();
        }

        public string GetHashHex(byte[] data)
        {
            var bodyHash = SHA256.HashData(data);
            return GetHex(bodyHash);
        }

        public string GetHex(byte[] data)
        {
            var builder = new StringBuilder(2 * data.Length);
            for (int i = 0; i < data.Length; i++)
            {
                builder.Append(data[i].ToString("x2", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }
    }
}