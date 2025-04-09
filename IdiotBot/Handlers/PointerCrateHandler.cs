using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection.Emit;
using Discord;

namespace IdiotBot.Handlers;

public class PointerCrateHandler
{
    private HttpClient client;

    public PointerCrateHandler()
    {
        client = new HttpClient();
    }

    public static Embed CreateListEmbed(GDLevel[] levels, out ComponentBuilder buttonComponentBuilder)
    {
        EmbedFieldBuilder[] fields = new EmbedFieldBuilder[levels.Length];

        for (int i = 0; i < levels.Length; i++)
        {
            fields[i] = new EmbedFieldBuilder()
                .WithName($"{levels[i].Position}. {levels[i].Name}")
                .WithValue($"Level Id: {levels[i].LevelId}\n{levels[i].Thumbnail}\n{levels[i].Video}\nPublisher: {levels[i].Publisher}\nVerifier: {levels[i].Verifier}");
        }

        buttonComponentBuilder = new ComponentBuilder().WithButton("Show More", customId: "show_more_button", ButtonStyle.Primary);

        return new EmbedBuilder()
            .WithTitle("Demon List")
            .WithDescription("PointerCrate")
            .WithFields(fields)
            .Build();
    }

    public async Task<GDLevel[]> RequestLevels(int start, int count)
    {
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
