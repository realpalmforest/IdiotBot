![IdiotBot Image](https://cdn.discordapp.com/app-icons/1279023749089333280/fc1f95fc41e9de99eb0af78db2e2f521.png?size=256)
# IdiotBot

**IdiotBot** is a basic Discord bot built with [Discord.Net](https://github.com/discord-net/Discord.Net) that I made for myself and some friends.

## ✨ Features

- **AI Chatbot**
  
  AI chatbot hosted locally using [Ollama](https://ollama.com) with an independent chat in each channel.

- **Wordle**
  
  Wordle recreation directly inside of Discord, which is capable of running an independent session in each channel.

- **Slash Commands**
  
  Miscellaneous slash commands for controlling Wordle games, flipping a coin, and wiping the AI memory in a channel.

## 🚀 Running the Bot

To run IdiotBot locally:

1. Clone the repository.
2. Install Ollama and pull the preferred model
3. Create a bot application on the [Discord Developer Portal](https://discord.com/developers/applications)
4. Save the application's bot token to a token.txt file in the Resources folder
5. Make sure all dependencies are installed and build with:

```bash
dotnet run
