using Discord;
using Discord.WebSocket;
using IdiotBot.Handlers;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdiotBot;

public static class Program
{
    public static DiscordSocketClient Client;

    public static Random Random = new Random();

    public static AIHandler AI;
    public static CommandHandler Commands;
    public static WordleHandler Wordle;
    public static PointerCrateHandler PointerCrate;

    public static Config Config;
    public static string ResourcesPath => Config.ResourcesConfig.ResourcesPath;

    public static async Task Main()
    {
        Config = Config.Load("config.json");

        Client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents =
                GatewayIntents.Guilds |
                GatewayIntents.GuildMessages |
                GatewayIntents.MessageContent
        });
        Client.Log += Log;

        AI = new AIHandler();
        Commands = new CommandHandler();
        Wordle = new WordleHandler();


        Client.Ready += ClientReady;

        await Client.LoginAsync(TokenType.Bot, Config.DiscordConfig.Token);
        await Client.StartAsync();

        Client.MessageReceived += MessageReceived;
        Client.ButtonExecuted += HandleButtonPressAsync;

        if (Commands != null)
            Client.SlashCommandExecuted += Commands.HandleSlashCommand;

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }


    // Override global application commands when the bot loads
    public static async Task ClientReady()
    {
        ApplicationCommandProperties[] commands =
        [
            new SlashCommandBuilder()
                .WithName("wipe-memory")
                .WithDescription("Wipes the AI's memory for this channel")
                .Build(),

            new SlashCommandBuilder()
                .WithName("coin-flip")
                .WithDescription("Flips a coin")
                .Build(),

            new SlashCommandBuilder()
                .WithName("wordle")
                .WithDescription("Starts a new Wordle game in this channel")
                .Build(),

            new SlashCommandBuilder()
                .WithName("wordle-end")
                .WithDescription("Ends the current Wordle game in this channel if there is one")
                .Build()
        ];

        await Client.BulkOverwriteGlobalApplicationCommandsAsync(commands);

        Print("Bot User: " + Client.CurrentUser.Mention);

        StringBuilder builder = new StringBuilder("Active Servers: ");
        foreach (var guild in Client.Guilds)
            builder.Append(guild.Name + (Client.Guilds.Last() == guild ? "" : ", "));
        Print(builder.ToString());
    }



    private static async Task MessageReceived(SocketMessage message)
    {
        // Uncomment this to log all messages received
        // string messageFormatted = $"{(message.Author.GlobalName is null ? message.Author.Username : message.Author.GlobalName)}:\t{message.Content}";
        // Print($"[{message.Channel.Name}]  " + messageFormatted);

        if (Wordle != null && Wordle.TryGuess(message))
            return;

        // A discarded task is created due to discord throwing an exception if a handler takes too long to respond
        _ = Task.Run(() => TryGetAIResponse(message));
    }



    private static async Task TryGetAIResponse(SocketMessage message)
    {
        if (AI == null)
            return;

        // Check if the message is replying to the bot
        bool isReplyToBot = false;
        if (message.Reference is not null)
        {
            var referencedMessage = await message.Channel.GetMessageAsync(message.Reference.MessageId.Value);
            isReplyToBot = referencedMessage != null && referencedMessage.Author.Id == Client.CurrentUser.Id;
        }

        // If the message is replying to or mentioning the bot, call to an AI response
        if (message.Content.Contains(Client.CurrentUser.Mention) || isReplyToBot)
            await RespondWithAi(message);
    }

    private static async Task RespondWithAi(SocketMessage message)
    {
        // Replace all mentioned user tags with their names
        string formattedMessage = message.Content;
        foreach (var user in message.MentionedUsers)
            formattedMessage = formattedMessage.Replace(user.Mention, user.GlobalName is null ? user.Username : user.GlobalName);

        string author = message.Author.GlobalName == null ? message.Author.Username : message.Author.GlobalName;

        await message.Channel.SendMessageAsync(await AI.GetResponse(formattedMessage, message.Channel.Id, author), messageReference: new MessageReference(message.Id));
    }

    private static async Task HandleButtonPressAsync(SocketMessageComponent component)
    {
        switch (component.Data.CustomId)
        {
            case "show_more_button":
                if (PointerCrate == null)
                    await component.RespondAsync("PointerCrate functionality is not enabled.");
                else
                    await component.RespondAsync("if i was bothered this would show more results :3");
                return;
        }
    }


    private static Task Log(LogMessage msg)
    {
        Print(msg.Message);
        return Task.CompletedTask;
    }
    public static void Print(string content) => Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]  {content}");
}