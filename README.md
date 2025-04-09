![IdiotBot Image](https://cdn.discordapp.com/app-icons/1279023749089333280/fc1f95fc41e9de99eb0af78db2e2f521.png?size=256)
# IdiotBot

**IdiotBot** is a basic Discord bot built with [Discord.Net](https://github.com/discord-net/Discord.Net) that I made for myself and some friends.

## ✨ Features

- **AI Chatbot**
  
  AI chatbot hosted locally using [Ollama](https://ollama.com) with an independent chat in each channel.

- **Wordle**
  
  Wordle recreation directly inside of Discord, which is capable of running an independent session in each channel.

- **GD Demon List**
  
  Cycle through the Geometry Dash demon list which is fetched from the [PointerCrate Rest API](https://pointercrate.com/documentation/index).

## ⚙ Tools
- [Discord Developer Portal](https://discord.com/developers/applications) - Discord application
- [Discord.NET](https://github.com/discord-net/Discord.Net) - Discord Bot functionality.
- [Ollama](https://github.com/ollama/ollama) - Software for hosting AI models locally.
- [OllamaSharp](https://github.com/awaescher/OllamaSharp) - For making Ollama API requests.
- [PointerCrate Rest API](https://pointercrate.com/documentation/index) - Geometry Dash demon list

## 🚀 Running the Bot

To run IdiotBot locally:

1. Clone the repository.
2. Install Ollama and pull a model (default is llama3.2)
3. Create a bot application on the [Discord Developer Portal](https://discord.com/developers/applications)
4. Create a copy of `config-example.json`, rename it to `config.json`, and configure settings
5. Make sure all dependencies are installed and build with:

```bash
dotnet run
