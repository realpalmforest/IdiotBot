using OllamaSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IdiotBot
{
    public class AIHandler
    {
        private readonly string instructionsPath = @"Data\instructions.txt";
        private readonly string model = "qwen2.5:0.5b"; // if reasoning quality suffers, switch back to llama3.2

        private OllamaApiClient client;
        private Dictionary<ulong, Chat> chats = new();

        public AIHandler()
        {
            client = new OllamaApiClient("http://localhost:11434", model);

            client.CreateModelAsync(new OllamaSharp.Models.CreateModelRequest()
            {
                Model = "idiot-bot",

                From = model,
                System = File.ReadAllText(instructionsPath)
            });

            client.SelectedModel = "idiot-bot";
        }


        public async Task<string> GetResponse(string prompt, ulong channelId)
        {
            string response = string.Empty;
            await foreach (var stream in GetChat(channelId).SendAsync(prompt))
                response += stream;


            return response;
        }

        public Chat GetChat(ulong channelId)
        {
            if (chats.ContainsKey(channelId))
                return chats.GetValueOrDefault(channelId);
            else
            {
                Chat chat = new Chat(client);
                chat.Options = new() { NumPredict = 2000 };

                chats.Add(channelId, chat);
                return chat;
            }
        }
    }
}
