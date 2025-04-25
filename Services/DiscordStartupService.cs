using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace mochabot.Services
{
    public class DiscordStartupService(DiscordSocketClient discord, IConfiguration config, ILogger<DiscordSocketClient> logger) : IHostedService
    {
        private readonly DiscordSocketClient _discord = discord;
        private readonly IConfiguration _config = config;
        private readonly ILogger<DiscordSocketClient> _logger = logger;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _discord.Log += message => LogUtil.LogAsync(_logger, message);

            await _discord.LoginAsync(TokenType.Bot, _config["Token"]);
            await _discord.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discord.LogoutAsync();
            await _discord.StopAsync();
        }
    }
}
