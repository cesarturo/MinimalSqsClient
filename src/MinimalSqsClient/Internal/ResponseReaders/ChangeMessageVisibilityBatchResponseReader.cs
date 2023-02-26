using System.Xml;

namespace MinimalSqsClient.Internal.ResponseReaders;

public static class ChangeMessageVisibilityBatchResponseReader
{
    public static List<(int id, bool success)> Read(HttpResponseMessage response, int count)
    {
        using var reader = XmlReader.Create(response.Content.ReadAsStream(), new XmlReaderSettings { IgnoreWhitespace = true });

        reader.ReadStartElement("ChangeMessageVisibilityBatchResponse");
        while (reader.Name is not "ChangeMessageVisibilityBatchResult" & reader.Read()) { }

        var result = new List<(int, bool)>(count);
        do
        {
            var elementName = reader.Name;
            if (elementName is "ChangeMessageVisibilityBatchResultEntry" or "BatchResultErrorEntry"
                && reader.IsStartElement())
            {
                reader.Read();
                reader.ReadStartElement("Id");
                var id = reader.ReadContentAsInt();
                var success = elementName is "ChangeMessageVisibilityBatchResultEntry";
                result.Add((id, success));
            }

        } while (reader.Read());

        return result;
    }
}