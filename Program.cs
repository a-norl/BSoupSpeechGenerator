using System.Diagnostics;
using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace BSoupSpeechGenerator;
class Program
{
    static async Task Main(string[] args)
    {
        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = Environment.GetEnvironmentVariable("TEST_DISCORD_BOT_ID"),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });

        var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
        {
            StringPrefixes = new[] { ">" }
        });

        commands.RegisterCommands<DiscordCommand>();

        await discord.ConnectAsync();
        await Task.Delay(-1);
    }

}