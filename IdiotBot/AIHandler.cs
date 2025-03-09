using OllamaSharp;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace IdiotBot
{
    public class AIHandler
    {
        private readonly string instructionsPath = @"Data\instructions.txt";

        private OllamaApiClient client;
        private Dictionary<ulong, Chat> chats = new();

        public AIHandler()
        {
            client = new OllamaApiClient("http://localhost:11434", "llama3.2");

            client.CreateModelAsync(new OllamaSharp.Models.CreateModelRequest()
            {
                Model = "idiot-bot",

                From = "llama3.2",
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

        private Chat GetChat(ulong channelId)
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
