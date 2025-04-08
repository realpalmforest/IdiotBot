using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace IdiotBot.Handlers;

public class WordleHandler
{
    private readonly string validWordleWordsPath = @$"{Program.ResourcesPath}\valid-wordle-words.txt";
    private readonly string wordleAnswersPath = @$"{Program.ResourcesPath}\wordle-answers.txt";
    private readonly string allWordsPath = @$"{Program.ResourcesPath}\all-words.txt";

    public string[] ValidWordleWords;
    public string[] WordleAnswers;
    public string[] AllWords;

    private Dictionary<ulong, WordleGame> games = new();

    public WordleHandler()
    {
        ValidWordleWords = File.ReadAllLines(validWordleWordsPath);
        WordleAnswers = File.ReadAllLines(wordleAnswersPath);
        AllWords = File.ReadAllLines(allWordsPath);
    }

    public Embed NewGame(ulong channelId)
    {
        if(games.ContainsKey(channelId))
        {
            return new EmbedBuilder()
                .WithTitle("Wordle")
                .WithColor(Color.Red)
                .WithFields([
                    new EmbedFieldBuilder().WithValue("——————————————"),
                    new EmbedFieldBuilder().WithValue("There is already a Wordle game going on in this channel."),
                    new EmbedFieldBuilder().WithValue("Type a 5 letter word in this channel to guess!")
                ])
                .WithFooter("To end the current game use the /wordle-end command.")
                .Build();
        }

        games.Add(channelId, new WordleGame(this, channelId));

        return new EmbedBuilder()
            .WithTitle("Wordle")
            .WithFooter("Use the /wordle-end command to end the game.")
            .WithColor(Color.Green)
            .WithAuthor(Program.Client.CurrentUser)
            .WithFields([
                new EmbedFieldBuilder().WithValue("——————————————"),
                new EmbedFieldBuilder().WithValue("Started new Wordle game in this channel."),
                new EmbedFieldBuilder().WithValue("Type a 5 letter word in this channel to guess!")
            ])
            .Build();
    }

    public Embed EndGame(ulong channelId, SocketUser winner)
    {
        Embed embed;

        if(games.ContainsKey(channelId))
        {
            embed = new EmbedBuilder()
            .WithTitle("Wordle")
            .WithDescription(winner == null ? "The game has ended." : $"{winner.Mention} guessed the word!")
            .WithFooter("Use the /wordle command to start a new game.")
            .WithAuthor(winner == null ? Program.Client.CurrentUser : winner)
            .WithColor(Color.DarkRed)
            .WithFields([
                new EmbedFieldBuilder().WithValue("——————————————"),
                    new EmbedFieldBuilder().WithValue($"The word was {games.GetValueOrDefault(channelId).Answer}!")
            ])
            .Build();

            games.Remove(channelId);
        }
        else
        {
            embed = new EmbedBuilder()
            .WithTitle("Wordle")
            .WithDescription("There is no active Wordle game in this channel.")
            .WithColor(Color.DarkGrey)
            .WithFields([
                new EmbedFieldBuilder().WithValue("——————————————"),
                new EmbedFieldBuilder().WithValue("Use the /wordle command to start a game.")
            ])
            .Build();
        }
            
        return embed;
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

    private short guessCount = 0;

    private WordleHandler handler;
    private ulong id;

    private static readonly string greenChar = ":green_square:";
    private static readonly string yellowChar = ":yellow_square:";
    private static readonly string blackChar = ":large_black_square:";

    public WordleGame(WordleHandler handler, ulong id)
    {
        this.handler = handler;
        this.id = id;
        Answer = handler.WordleAnswers[Program.Random.Next(handler.WordleAnswers.Length)].ToLower();
    }

    public void Guess(SocketUser author, string guess)
    {
        guessCount++;

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


        bool isGray = result.Count(emoji => emoji == blackChar) > 3;
        bool isGreen = guess == Answer;
        Color color = isGreen ? Color.Green : (isGray ? Color.DarkGrey : Color.Gold);

        handler.GetTextChannel(id).SendMessageAsync(
            embed: new EmbedBuilder()
                .WithTitle("Guess #" + guessCount)
                .WithColor(color)
                .WithAuthor(author)
                .WithFields([
                    new EmbedFieldBuilder().WithValue("——————————————"),
                    new EmbedFieldBuilder().WithValue($"{GetStringAsEmojis(guess)}\n{string.Join("", result)}")
                ])
                .Build()
            );

        handler.EndGame(id, author);
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