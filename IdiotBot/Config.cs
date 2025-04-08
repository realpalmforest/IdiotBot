using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IdiotBot;

public class Config
{
    [JsonPropertyName("discord")]
    public DiscordConfig DiscordConfig { get; set; }

    [JsonPropertyName("ai")]
    public AIConfig AIConfig { get; set; }

    [JsonPropertyName("resources")]
    public ResourcesConfig ResourcesConfig { get; set; }

    public static Config Load(string configPath)
    {
        Config config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configPath));
        return config;
    }
}

public class DiscordConfig
{
    [JsonPropertyName("token")]
    public string Token { get; set; }
}

public class AIConfig
{
    [JsonPropertyName("model")]
    public string Model { get; set; }

    [JsonPropertyName("instructions-path")]
    public string InstructionsPath { get; set; }

    [JsonPropertyName("temperature")]
    public float Temperature { get; set; }

    [JsonPropertyName("max-tokens")]
    public int MaxTokens { get; set; }
}

public class ResourcesConfig
{
    [JsonPropertyName("resources-path")]
    public string ResourcesPath { get; set; }
}