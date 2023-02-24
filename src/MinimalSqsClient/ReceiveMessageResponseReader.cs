using System.Xml;

namespace MinimalSqsClient;

public static class ReceiveMessageResponseReader
{
    public static SqsMessage? ReadSqsMessage(HttpResponseMessage response)
    {
        using var reader = XmlReader.Create(response.Content.ReadAsStream(), new XmlReaderSettings { IgnoreWhitespace = true });

        reader.ReadStartElement("ReceiveMessageResponse");
        reader.ReadStartElement("ReceiveMessageResult");
        do
        {
            if (reader.Name is "Message" && reader.IsStartElement())
            {
                return ReadMessage(reader);
            }
        } while (reader.Read());

        return null;
    }
    public static List<SqsMessage> ReadSqsMessages(HttpResponseMessage response)
    {
        using var reader = XmlReader.Create(response.Content.ReadAsStream(), new XmlReaderSettings { IgnoreWhitespace = true });

        reader.ReadStartElement("ReceiveMessageResponse");
        reader.ReadStartElement("ReceiveMessageResult");
        var messages = new List<SqsMessage>();
        do
        {
            if (reader.Name is "Message" && reader.IsStartElement())
            {
                var message = ReadMessage(reader);
                messages.Add(message);
            }
        } while (reader.Read());

        return messages;
    }
    private static SqsMessage ReadMessage(XmlReader reader)
    {
        SqsMessage message = new SqsMessage() { MessageAttributes = new Dictionary<string, string>() };
        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "MessageId" when reader.IsStartElement():
                    reader.Read();
                    message.MessageId = reader.ReadContentAsString();
                    break;
                case "ReceiptHandle" when reader.IsStartElement():
                    reader.Read();
                    message.ReceiptHandle = reader.ReadContentAsString();
                    break;
                case "Body" when reader.IsStartElement():
                    reader.Read();
                    message.Body = reader.ReadContentAsString();
                    break;
                case "MessageAttribute" when reader.IsStartElement():
                    reader.ReadStartElement();
                    reader.ReadStartElement("Name");
                    string name = reader.ReadContentAsString();
                    reader.ReadEndElement();
                    reader.ReadStartElement("Value");
                    reader.ReadStartElement("StringValue");
                    message.MessageAttributes.Add(name, reader.ReadContentAsString());
                    break;
                case "Message":
                    return message;
            }
        }
        return message;
    }
}

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

        } while (reader.Read()) ;

        return result;
    }
}