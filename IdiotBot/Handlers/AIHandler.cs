using OllamaSharp;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace IdiotBot.Handlers
{
    public class AIHandler
    {
        private readonly string instructionsPath;
        private readonly string model;
        private readonly float temperature;
        private readonly int maxTokens;

        private OllamaApiClient client;
        private Dictionary<ulong, Chat> chats = new();

        public AIHandler()
        {
            instructionsPath = @$"{Program.Config.ResourcesConfig.ResourcesPath}\{Program.Config.AIConfig.InstructionsPath}";
            model = Program.Config.AIConfig.Model;
            temperature = Program.Config.AIConfig.Temperature;
            maxTokens = Program.Config.AIConfig.MaxTokens;

            client = new OllamaApiClient("http://localhost:11434");

            // client.CreateModelAsync(new OllamaSharp.Models.CreateModelRequest()
            //{
            //    Model = "idiot-bot12",
            //    From = model,
            //    System = File.ReadAllText(instructionsPath)
            //});

            client.SelectedModel = model;
        }


        public async Task<string> GetResponse(string prompt, ulong chatId)
        {
            Program.Print(prompt);

            StringBuilder response = new StringBuilder();
            await foreach (var stream in GetChat(chatId).SendAsync(prompt))
                response.Append(stream);

            return response.ToString();
        }

        public Chat GetChat(ulong channelId)
        {
            if (chats.ContainsKey(channelId))
                return chats.GetValueOrDefault(channelId);
            else
            {
                Chat chat = new Chat(client, File.ReadAllText(instructionsPath));
                chat.Options = new()
                {
                    NumPredict = maxTokens,
                    Temperature = temperature
                };

                chats.Add(channelId, chat);
                return chat;
            }
        }
    }
}
