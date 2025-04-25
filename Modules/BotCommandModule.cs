using Discord;
using Discord.Interactions;

namespace mochabot.Modules
{
    public class BotCommandModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("ping", "test command")]
        public async Task Ping() => await RespondAsync(text: $"That took {Context.Client.Latency}ms...", ephemeral: true);

        [SlashCommand("get-random-message", "Takes you to a random message in this channel!")]
        public async Task GetRandomMessage([Summary("scan", "The number of messages to scan. The higher this is, the longer it takes!")] int scan = 1000)
        {
            var list = await Context.Channel.GetMessagesAsync(limit: scan).FlattenAsync();
            var message = list.ElementAt(Random.Shared.Next(Math.Min(scan, list.Count())));
            await RespondAsync(text: $"[Click here to be taken to a random message!]({message.GetJumpUrl()})", ephemeral: true);
        }
    }
}
