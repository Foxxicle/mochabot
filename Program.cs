using System.ComponentModel;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace mochabot;

class Program
{
    private readonly DiscordSocketClient client;
    private Random rng;
    private CommandServiceConfig commandConfig;

    public Program()
    {
        rng = new Random();

        commandConfig = new CommandServiceConfig
        {
            DefaultRunMode = RunMode.Async
        };
        this.client = new DiscordSocketClient();
        this.client.MessageReceived += MessageHandler;
        this.client.Ready += SlashCommandSetup;
        this.client.SlashCommandExecuted += SlashCommandHandler;
    }

    private async Task SlashCommandHandler(SocketSlashCommand command)
    {
        switch (command.Data.Name)
        {
            default:
                await command.RespondAsync(ephemeral: true, text: command.Data.Name + " doesn't work at the moment!");
                break;
            case "get-random-message":
                await command.DeferAsync(ephemeral: true);
                GetRandomMessage(command);
                break;
        }
    }

    private async Task GetRandomMessage(SocketSlashCommand command)
    {
        Console.Write("\nGetting random message");
        int scan = 1000;
        try
        {
            scan = int.Parse(command.Data.Options.First().Value.ToString());
        }
        catch { }
        Console.Write("\n" + scan);
        var channel = await command.GetChannelAsync();
        string returnvalue;
        var list = await channel.GetMessagesAsync(limit: scan).FlattenAsync();
        var randomizedList = list.OrderBy(order => rng.Next());
        foreach (IMessage message in randomizedList)
        {
            try
            {
                returnvalue = message.GetJumpUrl();
                await command.ModifyOriginalResponseAsync(m => m.Content = "[Click here to be taken to a random message!](" + returnvalue + ")");
                return;
            }
            catch
            {
                await command.ModifyOriginalResponseAsync(m => m.Content = "The command encountered an error.");
                return;
            }

        }
    }

    private async Task SlashCommandSetup()
    {
        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var secretProvider = config.Providers.First();
        secretProvider.TryGet("Guild", out var guildID);

        var guild = client.GetGuild(ulong.Parse(guildID)); //Hardcoded to one server for now
        var RandomMessage = new SlashCommandBuilder();

        //await guild.DeleteApplicationCommandsAsync();

        RandomMessage
        .WithName("get-random-message")
        .WithDescription("Takes you to a random message in this channel!")
        .AddOption("scan", ApplicationCommandOptionType.Integer, "The number of messages to scan, the higher the longer it takes", false);

        try
        {
            await guild.CreateApplicationCommandAsync(RandomMessage.Build());
        }
        catch (ApplicationCommandException exception)
        {
            var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);

            Console.WriteLine(json);
        }
    }

    public async Task StartBotAsync()
    {
        this.client.Log += LogFuncAsync;

        var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();
        var secretProvider = config.Providers.First();
        secretProvider.TryGet("Token", out var token);

        await this.client.LoginAsync(TokenType.Bot, token);
        await this.client.StartAsync();
        await this.client.SetGameAsync("ideas and feedback :3", null, ActivityType.Listening);

        await Task.Delay(-1);

        async Task LogFuncAsync(LogMessage message) =>
            Console.Write(message.ToString());
    }

    private async Task MessageHandler(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        if (message.Channel.GetType() != typeof(SocketDMChannel)) return;

        string response = string.Empty;

        Console.Write("\n\n" + message.Author.GlobalName + ": \"" + message.ToString() + "\"");

        if (message.Content.Contains("pfp"))
        {
            response = "This is what you look like";
            await ReplyAsync(message, message.Author.GetDisplayAvatarUrl());
        }

        if (response == string.Empty)
        {
            response = "Hi " + message.Author.GlobalName.ToLower() + " :3";
        }
        await ReplyAsync(message, response);
        Console.Write("\n\nMochaBot: " + response);
    }

    private void StealPfp(object? sender, AsyncCompletedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private async Task ReplyAsync(SocketMessage message, string response) =>
        await message.Channel.SendMessageAsync(response);


    static async Task Main(string[] args)
    {
        var myBot = new Program();
        await myBot.StartBotAsync();
    }
}
