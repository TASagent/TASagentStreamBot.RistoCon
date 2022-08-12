using System.Text.Json;

namespace TASagentTwitchBot.RistoCon.API.RistoBubbles;

public class RistoBubblesConfig
{
    private static string ConfigFilePath => BGC.IO.DataManagement.PathForDataFile("Config", "RistoBubblesConfig.json");
    private static readonly object _lock = new object();

    public bool Enabled { get; set; } = true;
    public string RistoBubblesMachine { get; set; } = "192.168.20.142";
    public double DonationThreshold { get; set; } = 5.00;
    public double BlastDuration { get; set; } = 10;

    public static RistoBubblesConfig GetConfig(RistoBubblesConfig? defaultConfig = null)
    {
        RistoBubblesConfig config;
        if (File.Exists(ConfigFilePath))
        {
            //Load existing config
            config = JsonSerializer.Deserialize<RistoBubblesConfig>(File.ReadAllText(ConfigFilePath))!;
        }
        else
        {
            config = defaultConfig ?? new RistoBubblesConfig();
        }

        config.Serialize();

        return config;
    }

    public void Serialize()
    {
        lock (_lock)
        {
            File.WriteAllText(ConfigFilePath, JsonSerializer.Serialize(this));
        }
    }
}
