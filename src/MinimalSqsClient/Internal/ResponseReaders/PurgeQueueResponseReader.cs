using System.Xml;

namespace MinimalSqsClient.Internal.ResponseReaders;

public static class PurgeQueueResponseReader
{
    public static string ReadErrorCode(HttpResponseMessage response)
    {
        using var reader = XmlReader.Create(response.Content.ReadAsStream(), new XmlReaderSettings { IgnoreWhitespace = true });

        reader.ReadStartElement("ErrorResponse");
        reader.ReadStartElement("Error");
        reader.ReadStartElement("Type");
        reader.Read();
        reader.ReadEndElement();
        reader.ReadStartElement("Code");
        return reader.ReadContentAsString();
    }
}