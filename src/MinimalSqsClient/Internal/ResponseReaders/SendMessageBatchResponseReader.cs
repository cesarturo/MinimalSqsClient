using System.Xml;

namespace MinimalSqsClient.Internal.ResponseReaders;

public static class SendMessageBatchResponseReader
{
    public static string[] ReadMessageIds(HttpResponseMessage response, int count)
    {
        var result = new string[count];
        using var reader = XmlReader.Create(response.Content.ReadAsStream(), new XmlReaderSettings { IgnoreWhitespace = true });
        
        reader.ReadStartElement("SendMessageBatchResponse");
        reader.ReadStartElement("SendMessageBatchResult");
        do
        {
            var elementName = reader.Name;
            if (elementName is "SendMessageBatchResultEntry"
                && reader.IsStartElement())
            {
                reader.Read();
                reader.ReadStartElement("Id");
                var index = reader.ReadContentAsInt();
                reader.Read();
                reader.ReadStartElement("MessageId");
                var messageId = reader.ReadContentAsString();
                result[index] = messageId;
            }
        } while (reader.Read());

        return result;
    }
}