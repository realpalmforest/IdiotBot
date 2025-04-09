using Discord;
using IdiotBot.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IdiotBot.Handlers;

public class PointerCrateHandler
{
    public static byte LevelLoadAmount { get; set; } = 15;
    public Dictionary<ulong, int> ChannelLevels = new();

    private HttpClient client;
    private GDLevel[] loadedLevels;


    public PointerCrateHandler()
    {
        client = new HttpClient();
    }

    public async Task<Embed> CreateFirstEmbed(ulong channelId)
    {
        loadedLevels = await RequestLevels(0, LevelLoadAmount);

        if (ChannelLevels.ContainsKey(channelId))
            ChannelLevels.Remove(channelId);


        ChannelLevels.Add(channelId, 0);

        return await GetNextLevelEmbed(channelId);
    }

    public async Task<Embed> GetNextLevelEmbed(ulong channelId)
    {
        if (ChannelLevels[channelId] == loadedLevels.Last().Position)
        {
            loadedLevels = await RequestLevels(loadedLevels.Last().Position, LevelLoadAmount);
        }

        // index is one lower than the next so it stays the same
        ChannelLevels[channelId]++;

        int nextLevelIndex;

        if (ChannelLevels[channelId] == LevelLoadAmount)
            nextLevelIndex = LevelLoadAmount - 1;
        else nextLevelIndex = ChannelLevels[channelId] % LevelLoadAmount - 1;


        return new EmbedBuilder()
                .WithTitle($"{loadedLevels[nextLevelIndex].Position}. {loadedLevels[nextLevelIndex].Name} by {loadedLevels[nextLevelIndex].Publisher.Name}")
                .WithDescription($"Verifier: {loadedLevels[nextLevelIndex].Verifier.Name}")
                .WithFooter($"Level Id: {loadedLevels[nextLevelIndex].LevelId}")
                .WithImageUrl(loadedLevels[nextLevelIndex].Thumbnail)
                .WithUrl(loadedLevels[nextLevelIndex].Video)
                .WithColor(await ThumbnailToColor(loadedLevels[nextLevelIndex].Thumbnail, 50))
                .Build();
    }

    public async Task<Embed> GetPreviousLevelEmbed(ulong channelId)
    {
        if (ChannelLevels[channelId] == loadedLevels[0].Position)
        {
            loadedLevels = await RequestLevels(ChannelLevels[channelId] - LevelLoadAmount - 1, LevelLoadAmount);
        }

        // Check again to make sure the list changed (if it didn't the embed will be the same)
        if (ChannelLevels[channelId] != loadedLevels[0].Position)
            ChannelLevels[channelId]--;

        int previousLevelIndex;

        if (ChannelLevels[channelId] == LevelLoadAmount)
            previousLevelIndex = LevelLoadAmount - 1;
        else previousLevelIndex = ChannelLevels[channelId] % LevelLoadAmount - 1;


        return new EmbedBuilder()
                .WithTitle($"{loadedLevels[previousLevelIndex].Position}. {loadedLevels[previousLevelIndex].Name} by {loadedLevels[previousLevelIndex].Publisher.Name}")
                .WithDescription($"Verifier: {loadedLevels[previousLevelIndex].Verifier.Name}")
                .WithFooter($"Level Id: {loadedLevels[previousLevelIndex].LevelId}")
                .WithImageUrl(loadedLevels[previousLevelIndex].Thumbnail)
                .WithUrl(loadedLevels[previousLevelIndex].Video)
                .WithColor(await ThumbnailToColor(loadedLevels[previousLevelIndex].Thumbnail, 50))
                .Build();
    }

    private async Task<Color> ThumbnailToColor(string thumbnailUrl, int brightnessModifier = 0)
    {
        System.Drawing.Color thumbnailColor = Tools.GetAverageColor(await Tools.GetBitmapFromUrl(thumbnailUrl));
        return new Color(thumbnailColor.R + brightnessModifier, thumbnailColor.G + brightnessModifier, thumbnailColor.B + brightnessModifier);
    }

    private async Task<GDLevel[]> RequestLevels(int start, int count)
    {
        start = Math.Max(start, 0);
        count = Math.Clamp(count, 1, 100);

        var json = await GetAsyncResponse(client, $"https://pointercrate.com/api/v2/demons/listed/?limit={count}&after={start}");
        return GDLevel.LoadList(json);
    }

    private async Task<string> GetAsyncResponse(HttpClient httpClient, string url)
    {
        using HttpResponseMessage response = await httpClient.GetAsync(url);

        var jsonResponse = await response.Content.ReadAsStringAsync();
        return jsonResponse.ToString();
    }
}

public class GDLevel
{
    public string Name { get; set; }
    public int Id { get; set; }
    public int Position { get; set; }

    [JsonPropertyName("requirement")]
    public int ListRequirement { get; set; }

    [JsonPropertyName("level_id")]
    public int LevelId { get; set; }

    public string Video { get; set; }
    public string Thumbnail { get; set; }

    public GDUser Publisher { get; set; }
    public GDUser Verifier { get; set; }


    public static GDLevel[] LoadList(string json)
    {
        return JsonSerializer.Deserialize<GDLevel[]>(json, new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        });
    }
}

public class GDUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Banned { get; set; }
}
