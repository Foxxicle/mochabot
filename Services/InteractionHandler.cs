using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace mochabot.Services
{
    internal class InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, ILogger<InteractionService> logger) : IHostedService
    {
        private readonly DiscordSocketClient _client = client;
        private readonly InteractionService _handler = handler;
        private readonly IServiceProvider _services = services;
        private readonly ILogger<InteractionService> _logger = logger;

        private async Task HandleMessage(SocketMessage message)
        {
            if (message.Channel.GetChannelType() != ChannelType.DM || message is SocketSystemMessage || message.Author.IsBot)
            {
                await Task.CompletedTask;
            }
            else
            {
                if (message.CleanContent.Contains("pfp", StringComparison.CurrentCultureIgnoreCase))
                {
                    await message.Channel.SendMessageAsync(message.Author.GetDisplayAvatarUrl());
                    await message.Channel.SendMessageAsync("This is what you look like");
                }
                else
                {
                    await message.Channel.SendMessageAsync($"Hi {message.Author.GlobalName.ToLower()} :3");
                }
            }
        }

        private async Task HandleInteractionExecute(ICommandInfo commandInfo, IInteractionContext context, IResult result)
        {
            if (!result.IsSuccess)
            {
                switch (result.Error)
                {
                    case InteractionCommandError.UnknownCommand:
                        await context.Interaction.RespondAsync($"That command doesn't exist! ({result.ErrorReason})", ephemeral: true);
                        break;
                    case InteractionCommandError.ConvertFailed:
                        await context.Interaction.RespondAsync($"Conversion failed: {result.ErrorReason}", ephemeral: true);
                        break;
                    case InteractionCommandError.BadArgs:
                        await context.Interaction.RespondAsync($"Invalid arguments. ({result.ErrorReason})", ephemeral: true);
                        break;
                    case InteractionCommandError.Exception:
                        await context.Interaction.RespondAsync($"Command encountered an exception: {result.ErrorReason}", ephemeral: true);
                        break;
                    case InteractionCommandError.Unsuccessful:
                        await context.Interaction.RespondAsync($"Command could not be executed. {result.ErrorReason}", ephemeral: true);
                        break;
                    case InteractionCommandError.UnmetPrecondition:
                        await context.Interaction.RespondAsync($"Unmet precondition: {result.ErrorReason}", ephemeral: true);
                        break;
                    case InteractionCommandError.ParseFailed:
                        await context.Interaction.RespondAsync($"Parse failed: {result.ErrorReason}", ephemeral: true);
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task HandleInteraction(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);

                await _handler.ExecuteCommandAsync(context, _services);
            }
            catch
            {
                if (interaction.Type is InteractionType.ApplicationCommand)
                    await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
            }
        }

        private async Task ReadyAsync()
        {
            await _handler.RegisterCommandsToGuildAsync(1328492143429550101);
            await _client.SetGameAsync(name: "ideas and feedback :3", type: ActivityType.Listening);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Ready += ReadyAsync;
            _handler.Log += message => LogUtil.LogAsync(_logger, message);

            await _handler.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            _client.InteractionCreated += HandleInteraction;

            _handler.InteractionExecuted += HandleInteractionExecute;

            _client.MessageReceived += HandleMessage;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}