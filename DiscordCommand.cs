using System.Diagnostics;
using System.Text.RegularExpressions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace BSoupSpeechGenerator;

public class DiscordCommand : BaseCommandModule
{

    SceneGenerator generator = new();

    private async Task<string> fixMessageContent(CommandContext ctx, string message)
    {
        string fixedMessage = message;

        string pingPattern = @"<@(\d{18})>";
        var pingMatches = Regex.Matches(fixedMessage, pingPattern);
        foreach (Match match in pingMatches)
        {
            var user = await ctx.Guild.GetMemberAsync(ulong.Parse(match.Groups[1].Value));
            var userName = user.DisplayName;
            fixedMessage = fixedMessage.Replace(match.Groups[0].Value, $"@{userName}");
        }

        string rolePattern = @"<@&(\d{18})>";
        var roleMatches = Regex.Matches(fixedMessage, rolePattern);
        foreach (Match match in roleMatches) {
            var role = ctx.Guild.GetRole(ulong.Parse(match.Groups[1].Value));
            var roleName = role.Name;
            fixedMessage = fixedMessage.Replace(match.Groups[0].Value, $"@{roleName}");
        }

        string emotePattern = @"<:(\w+):(\d{18})>";
        var emoteMatches = Regex.Matches(fixedMessage, emotePattern);
        foreach (Match match in emoteMatches)
        {
            var emote = await ctx.Guild.GetEmojiAsync(ulong.Parse(match.Groups[2].Value));
            var emoteName = emote.Name;
            fixedMessage = fixedMessage.Replace(match.Groups[0].Value, emoteName);
        }
        return fixedMessage;

    }

    [Command("soup"), Aliases("soupmp4")]
    public async Task SoupGeneratorMP4(CommandContext ctx, int farBack)
    {
        var timer = new Stopwatch();
        var typing = ctx.TriggerTypingAsync();
        timer.Start();

        if (farBack < 1 || farBack > 25)
        {
            await ctx.RespondAsync("did you know akarsha hates you");
            return;
        }
        var generatingMessage = ctx.RespondAsync("Generating your video...");
        var messages = ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id, farBack);
        List<(string, string)> script = new();
        foreach (var message in await messages)
        {

            script.Add((((await ctx.Guild.GetMemberAsync(message.Author.Id)).DisplayName), await fixMessageContent(ctx, message.Content)));
        }
        script.Reverse();
        var sceneStream = generator.GenerateMP4NoIntermediary(script);
        timer.Stop();
        DiscordMessageBuilder reply = new DiscordMessageBuilder()
            .WithContent($"Generated in {timer.ElapsedMilliseconds}ms");
        reply.AddFile("bsoupmessage.mp4", sceneStream);
        await typing;
        sceneStream.Position = 0;
        Console.WriteLine("beginning send");
        var replySend = ctx.RespondAsync(reply);
        var generatedMessage = await generatingMessage;
        await replySend;
        await generatedMessage.DeleteAsync();
        Console.WriteLine("sent");
        File.Delete(sceneStream.Name);
        sceneStream.Close();
    }

}
