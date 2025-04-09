using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace IdiotBot.Handlers;

public class CommandHandler
{
    public async Task HandleSlashCommand(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            case "wipe-memory":
                Program.AI.GetChat(command.Channel.Id).Messages.Clear();
                await command.RespondAsync("Wiped AI memory for this channel.");
                return;
            case "coin-flip":
                string response = "you're days are numbered <:boneguy:1348174676249284691>";
                short num = (short)Program.Random.Next(101);

                if (num < 50) response = "Heads";
                else if (num > 50) response = "Tails";

                await command.RespondAsync(response);
                return;
            case "wordle":
                if (Program.Wordle == null)
                    await command.RespondAsync("Wordle is not currently enabled. :c");
                else
                    await command.RespondAsync(embed: Program.Wordle.StartGame(command.User, command.Channel.Id));
                return;
            case "wordle-end":
                if (Program.Wordle == null)
                    await command.RespondAsync("Wordle is not currently enabled. :c");
                else
                    await command.RespondAsync(embed: Program.Wordle.StopGame(command.Channel.Id, command.User));
                return;
            case "demon-list":
                if (Program.PointerCrate == null)
                    await command.RespondAsync("PointerCrate functionality is not currently enabled. :c");
                else
                {
                    ComponentBuilder buttons = new ComponentBuilder()
                        .WithButton("< Previous", customId: "previous_button", ButtonStyle.Success)
                        .WithButton("Next >", customId: "next_button", ButtonStyle.Success);

                    await command.RespondAsync(
                        embed: await Program.PointerCrate.CreateFirstEmbed(command.Channel.Id),
                        components: buttons.Build()
                        );
                }
                return;
        }
    }
}