using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

using System.Linq;

namespace DiscordChatDumper;

public class Bot
{
    private DiscordClient DiscordClient { get; }
    private const string DUMP_DIRECTORY = "dumps";
    
    public Bot(string secret)
    {
        DiscordClient = new DiscordClient(new()
        {
            Token = secret,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            MinimumLogLevel = LogLevel.Error
        });
    }

    public async Task ConnectAsync()
    {
        await DiscordClient.ConnectAsync();
    }

    public async Task<IReadOnlyList<DiscordChannel>> GetAllTextChannelsAsync(ulong guildId)
    {
        DiscordGuild guild;
        
        try
        {
            guild = await DiscordClient.GetGuildAsync(guildId);
        }
        catch (NotFoundException)
        {
            Console.WriteLine("Error. Couldn't find specified guild");
            return null;
        }

        return await guild.GetChannelsAsync();
    }

    public async Task DumpChatAsync(ulong guildId, ulong id)
    {
        DiscordGuild guild;
        
        try
        {
            guild = await DiscordClient.GetGuildAsync(guildId);
        }
        catch (NotFoundException)
        {
            Console.WriteLine("Error. Couldn't find specified guild");
            return;
        }

        DiscordChannel channel;

        try
        {
            channel = guild.GetChannel(id);
        }
        catch (ServerErrorException)
        {
            Console.WriteLine("Couldn't get specified channel");
            return;
        }

        IReadOnlyList<DiscordMessage> messages;
        try
        {
            messages = await channel.GetMessagesAsync();
        }
        catch (UnauthorizedException)
        {
            Console.WriteLine("Couldn't get messages from specified chat. No permissions.");
            return;
        }
        
        if (!Directory.Exists(DUMP_DIRECTORY))
        {
            Directory.CreateDirectory(DUMP_DIRECTORY);
        }

        string guildDirectory = Path.Combine(DUMP_DIRECTORY, guildId.ToString());
        if (!Directory.Exists(guildDirectory))
        {
            Directory.CreateDirectory(guildDirectory);
        }
        
        string chatPath = Path.Combine(guildDirectory, $"{guildId}-{id}-{DateTime.Now.Ticks}.json");
        MessageWriter writer = new(chatPath);
        
        Console.WriteLine($"Chat will be saved in: {chatPath}");
        
        Console.WriteLine();

        DiscordMessage lastMessage = null;
        while (messages is not null && messages.Count != 0)
        {
            lastMessage = messages.Last();
            writer.WriteMessages(messages);
            messages = await channel.GetMessagesBeforeAsync(lastMessage.Id);
        }
        
        writer.Close();
    }
}