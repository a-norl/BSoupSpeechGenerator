using DSharpPlus;
using DSharpPlus.CommandsNext;

namespace BSoupSpeechGenerator;
class Program
{
    static async Task Main(string[] args)
    {
        var discord = new DiscordClient(new DiscordConfiguration()
        {
            Token = Environment.GetEnvironmentVariable("BUTTERFLY_SOUP_TOKEN"),
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        });

        var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
        {
            StringPrefixes = new[] { "!" },
            EnableDefaultHelp = false
        });

        commands.RegisterCommands<DiscordCommand>();

        await discord.ConnectAsync();
        await Task.Delay(-1);
    }

}