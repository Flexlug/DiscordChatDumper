using DSharpPlus.Entities;
using Newtonsoft.Json;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace DiscordChatDumper;

public class MessageWriter
{
    private StreamWriter _writer;
    private long _messageCount = 0;
    
    public MessageWriter(string path)
    {
        _writer = new(path);
        _writer.AutoFlush = true;
        _writer.WriteLine('[');
    }

    public void Close()
    {
        _writer.Write(']');
        _writer.Close();
        _writer.Dispose();
        
        Console.WriteLine("Done");
    }

    public void WriteMessages(IReadOnlyList<DiscordMessage> messages)
    {   
        if (_messageCount != 0)
            _writer.Write(',');

        var lastIndex = messages.Count - 1;
        for (int i = 0; i < messages.Count; i++)
        {
            var strMessage = JsonConvert.SerializeObject(messages[i], new JsonSerializerSettings()
            {
                Error = HandleDeserializationError
            });
            _writer.Write(strMessage);

            if (i != lastIndex)
                _writer.WriteLine(',');
        }
        _messageCount += messages.Count;
        Console.WriteLine($"Wrote {_messageCount} messages...");
    }

    private void HandleDeserializationError(object? sender, ErrorEventArgs errorArgs)
    {
        var currentError = errorArgs.ErrorContext.Error.Message;
        errorArgs.ErrorContext.Handled = true;
    }
}