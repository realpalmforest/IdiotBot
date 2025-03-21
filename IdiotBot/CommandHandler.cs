using Discord.WebSocket;
using System.Threading.Tasks;

namespace IdiotBot;

public static class CommandHandler
{
    public static async Task SlashCommandHandler(SocketSlashCommand command)
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
        }
    }
}
