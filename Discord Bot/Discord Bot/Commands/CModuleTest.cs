using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    internal class CModuleTest : BaseCommandModule
    {
        public Random Rng { private get; set; }

        //[Command("greet")]
        //public async Task GreetCommand(CommandContext ctx, [RemainingText] string name)
        //{
        //    await ctx.RespondAsync($"Greetings, {string.Join(" ", name)}!");
        //}

        //[Command("greet")]
        //public async Task GreetCommand(CommandContext ctx, DiscordMember discordMember)
        //{
        //    await ctx.Channel.SendMessageAsync($"Greetings, {string.Join(" ", discordMember.Mention)}!");
        //}

        [Command("random")]
        public async Task RandomCommand(CommandContext ctx, int min, int max)
        {
            await ctx.RespondAsync($"Your number is {Rng.Next(min, max)}");
        }

        [Command("testbutton")]
        public async Task TestButton(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder()
            .WithContent("This message has buttons! Pretty neat innit?")
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, "1_top", "Blurple!"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "2_top", "Grey!"),
                new DiscordButtonComponent(ButtonStyle.Success, "3_top", "Green!"),
                new DiscordButtonComponent(ButtonStyle.Danger, "4_top", "Red!"),
                new DiscordLinkButtonComponent("https://some-super-cool.site", "Link!")
            });

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                await e.Interaction.CreateResponseAsync(
        InteractionResponseType.UpdateMessage,
        new DiscordInteractionResponseBuilder()
            .WithContent("No more buttons for you >:)"));
            };

            await ctx.RespondAsync(builder);
        }
    }
}