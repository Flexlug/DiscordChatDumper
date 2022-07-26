using Newtonsoft.Json;

namespace DiscordChatDumper;

public class Settings
{
    public string Secret { get; set; }
    public ulong? GuildId { get; set; }
    public ulong? ChannelId { get; set; }

    public static Settings Load(string path)
    {
        string rawString = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<Settings>(path);
    }
}