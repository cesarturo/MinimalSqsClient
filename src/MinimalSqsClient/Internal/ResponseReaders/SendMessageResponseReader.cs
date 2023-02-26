using System.Xml;

namespace MinimalSqsClient.Internal.ResponseReaders;

public static class SendMessageResponseReader
{
    public static string ReadMessageId(HttpResponseMessage response)
    {
        using var reader = XmlReader.Create(response.Content.ReadAsStream(), new XmlReaderSettings { IgnoreWhitespace = true });

        reader.ReadStartElement("SendMessageResponse");
        reader.ReadStartElement("SendMessageResult");
        do
        {
            if (reader.Name is "MessageId" && reader.IsStartElement())
            {
                reader.Read();
                return reader.ReadContentAsString();
            }
        } while (reader.Read());

        throw new Exception("SedMessage Response Error.");
    }
}