using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Cocona;
using DiscordChatDumper;
using DSharpPlus.Entities;
using Newtonsoft.Json;

var app = CoconaApp.Create();

app.AddSubCommand("dump", x =>
{
    x.AddCommand("guild", async () =>
    {
        Settings settings = Settings.Load(Path.Combine("config", "settings.json"));
        string token = settings.Secret ?? throw new Exception("No token provided");
        if (!settings.GuildId.HasValue)
        {
            throw new Exception("No guild id provided");
        }
        ulong guildId = settings.GuildId.Value;
        
        Bot bot = new(token);

        try
        {
            await bot.ConnectAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection error: {e.Message}");
            return;
        }

        Console.WriteLine("Connected");

        Stopwatch guildDumpTime = new();
        Stopwatch channelDumpTime = new();
        var channels = await bot.GetAllTextChannelsAsync(guildId);
        int counter = 1;
        guildDumpTime.Start();
        foreach (var channel in channels)
        {
            if (channel.IsCategory)
            {
                Console.WriteLine($"Channel {channel.Id} is category. Skipping. ({counter++}/{channels.Count})");
                continue;
            }

            if (channel.Bitrate is not null)
            {
                Console.WriteLine($"Channel {channel.Id} is voice channel. Skipping. ({counter++}/{channels.Count})");
                continue;
            }

            channelDumpTime.Restart();
            await bot.DumpChatAsync(guildId, channel.Id);
            channelDumpTime.Stop();

            Console.WriteLine($"Done. ({counter++}/{channels.Count}). Elapsed: {channelDumpTime.Elapsed}");
        }

        guildDumpTime.Stop();
        Console.WriteLine($"Guild dump done! Elapsed: {guildDumpTime.Elapsed}");
    });

    x.AddCommand("channel", async () =>
    {   
        Settings settings = Settings.Load(Path.Combine("config", "settings.json"));
        string token = settings.Secret ?? throw new Exception("No token provided");
        if (!settings.ChannelId.HasValue)
        {
            throw new Exception("No guild id provided");
        }
        ulong guildId = settings.GuildId.Value;
        if (!settings.ChannelId.HasValue)
        {
            throw new Exception("No channel id provided");
        }
        ulong channelId = settings.ChannelId.Value;
        
        Bot bot = new(token);

        try
        {
            await bot.ConnectAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine($"Connection error: {e.Message}");
            return;
        }

        Console.WriteLine("Connected");

        Stopwatch channelDumpTime = new Stopwatch();
        channelDumpTime.Start();

        await bot.DumpChatAsync(guildId, channelId);

        channelDumpTime.Stop();
        Console.WriteLine($"Elapsed: {channelDumpTime.Elapsed}");

    }).WithDescription("Dump chat to a text file");
});


app.AddCommand("validate", async (
    [Option(Description = "Path to text file with dumped chat")]
    string path) =>
{
    StreamReader reader = new(path);
    var rawFile = reader.ReadToEnd();
    var messages = JsonConvert.DeserializeObject<List<DiscordMessage>>(rawFile);

    Console.WriteLine($"Messages count: {messages.Count}");
    Console.WriteLine("Done");
}).WithDescription("Validate text file with dumper chat");

app.Run();