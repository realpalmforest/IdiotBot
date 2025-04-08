using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IdiotBot.Handlers;

public class WordleHandler
{
    private readonly string validWordleWordsPath = @$"{Program.ResourcesPath}\valid-wordle-words.txt";
    private readonly string wordleAnswersPath = @$"{Program.ResourcesPath}\wordle-answers.txt";
    private readonly string allWordsPath = @$"{Program.ResourcesPath}\all-words.txt";

    public string[] ValidWordleWords;
    public string[] WordleAnswers;
    public string[] AllWords;

    public Dictionary<ulong, WordleGame> Games = new();

    public WordleHandler()
    {
        ValidWordleWords = File.ReadAllLines(validWordleWordsPath);
        WordleAnswers = File.ReadAllLines(wordleAnswersPath);
        AllWords = File.ReadAllLines(allWordsPath);
    }

    public Embed StartGame(SocketUser startUser, ulong channelId)
    {
        // If there is already a wordle game running
        if (Games.ContainsKey(channelId))
        {
            return new EmbedBuilder()
                .WithTitle("Wordle")
                .WithDescription("There is already a Wordle game going on in this channel.")
                .WithColor(Color.Red)
                .WithFields([new EmbedFieldBuilder().WithName("——————————————").WithValue("Type a 5 letter word to guess!")])
                .WithFooter("To end the current game use the /wordle-end command.")
                .Build();
        }

        Games.Add(channelId, new WordleGame(this, channelId));

        return new EmbedBuilder()
            .WithTitle("Wordle")
            .WithDescription("Started new Wordle game in this channel.")
            .WithFooter("\nUse the /wordle-end command to end the game.")
            .WithAuthor(startUser)
            .WithColor(Color.Green)
            .WithFields([new EmbedFieldBuilder().WithName("——————————————").WithValue("Type a 5 letter word to guess!")])
            .Build();
    }

    public Embed StopGame(ulong channelId, SocketUser endUser)
    {
        Embed embed;

        // If there is a game to end
        if (Games.ContainsKey(channelId))
        {
            embed = new EmbedBuilder()
            .WithTitle("Wordle")
            .WithDescription("The game has ended.")
            .WithFooter("Use the /wordle command to start a new game.")
            .WithAuthor(endUser)
            .WithColor(Color.DarkTeal)
            .WithFields([new EmbedFieldBuilder().WithName("——————————————").WithValue($"The word was **{Games.GetValueOrDefault(channelId).Answer}**!")])
            .Build();

            Games.Remove(channelId);
        }
        // If there is no game to end
        else
        {
            embed = new EmbedBuilder()
            .WithTitle("Wordle")
            .WithDescription("There is no active Wordle game in this channel.")
            .WithColor(Color.DarkGrey)
            .WithFooter("Use the /wordle command to start a game.")
            .Build();
        }

        return embed;
    }

    /// <summary>Returns true if the guess was successful</summary>
    public bool TryGuess(SocketMessage message)
    {
        if (!Games.ContainsKey(message.Channel.Id))
            return false;

        if (message.Author.IsBot)
            return false;

        if (message.Content.ToLower().Length != 5)
            return false;

        if (!ValidWordleWords.Contains(message.Content.ToLower()))
            return false;

        Games.GetValueOrDefault(message.Channel.Id).Guess(message);
        return true;
    }

    public SocketTextChannel GetTextChannel(ulong channelId)
    {
        var channel = Program.Client.GetChannel(channelId);

        if (channel != null && channel.ChannelType == Discord.ChannelType.Text || channel.ChannelType == Discord.ChannelType.DM)
        {
            return channel as SocketTextChannel;
        }

        return null;
    }
}

public class WordleGame
{
    public readonly string Answer;
    public short GuessCount = 0;

    private WordleHandler handler;
    private ulong id;

    private static readonly string greenChar = ":green_square:";
    private static readonly string yellowChar = ":yellow_square:";
    private static readonly string blackChar = ":black_large_square:";

    public WordleGame(WordleHandler handler, ulong id)
    {
        this.handler = handler;
        this.id = id;
        Answer = handler.WordleAnswers[Program.Random.Next(handler.WordleAnswers.Length)].ToLower();
    }

    public async void Guess(SocketMessage message)
    {
        string guess = message.Content.ToLower();

        GuessCount++;

        if (guess.Length != 5)
            throw new ArgumentException("Guess must be 5 letters long");

        guess = guess.ToLower();
        string[] result = [blackChar, blackChar, blackChar, blackChar, blackChar];
        bool[] answerUsed = [false, false, false, false, false];

        // First pass checks for any correct placed letters and marks
        // The answer letters as used for the second pass
        for (int i = 0; i < guess.Length; i++)
        {
            if (guess[i] == Answer[i])
            {
                result[i] = greenChar;
                answerUsed[i] = true;
            }
        }

        for (int i = 0; i < guess.Length; i++)
        {
            if (result[i] == greenChar)
                continue;

            for (int j = 0; j < guess.Length; j++)
            {
                // If the letters match and the letter of the answer has not been used
                // Then mark the letter of the answer as used ans set the guess letter to yellow
                if (guess[i] == Answer[j] && !answerUsed[j])
                {
                    result[i] = yellowChar;
                    answerUsed[j] = true;
                    break;
                }
            }
        }


        bool isGameOver = guess == Answer;
        bool isMostlyWrong = result.Count(emoji => emoji == blackChar) > 2;
        Color color = isGameOver ? Color.Green : (isMostlyWrong ? Color.DarkGrey : Color.Gold);

        await message.Channel.SendMessageAsync(
            embed: new EmbedBuilder()
                .WithTitle("Guess #" + GuessCount)
                .WithColor(color)
                .WithAuthor(message.Author)
                .WithFields([new EmbedFieldBuilder().WithName("——————————————").WithValue($"{GetStringAsEmojis(guess)}\n{string.Join("", result)}")])
                .Build(),
            messageReference: new MessageReference(message.Id)
            );

        // If this guess is correct end the game send embed and end game
        if (isGameOver)
        {
            await message.Channel.SendMessageAsync(embed: new EmbedBuilder()
            .WithTitle("Wordle")
            .WithDescription($"{message.Author.Mention} guessed the word in {GuessCount}!")
            .WithFooter("Use the /wordle command to start a new game.")
            .WithAuthor(message.Author)
            .WithColor(Color.Teal)
            .WithFields([new EmbedFieldBuilder().WithName("——————————————").WithValue($"The word was **{Answer}**!")])
            .Build());

            handler.Games.Remove(id);
        }
    }


    private string GetStringAsEmojis(string word)
    {
        StringBuilder result = new StringBuilder();
        word = word.ToLower();

        foreach (char c in word)
        {
            result.Append($":regional_indicator_{c}:");
        }

        return result.ToString();
    }
}