using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using DSharpPlus.Entities;

namespace Tiny_Bot.Commands
{
    class MyFirstModule : BaseCommandModule
    {
        public Random Rng { private get; set; }

        [Command("greet")]
        public async Task GreetCommand(CommandContext ctx, [RemainingText] string name)
        {
            await ctx.RespondAsync($"Greetings, {string.Join(" ", name)}!");
        }

        [Command("greet")]
        public async Task GreetCommand(CommandContext ctx, DiscordMember discordMember)
        {
            await ctx.Channel.SendMessageAsync($"Greetings, {string.Join(" ", discordMember.Mention)}!");
        }

        [Command("random")]
        public async Task RandomCommand(CommandContext ctx, int min, int max)
        {
            await ctx.RespondAsync($"Your number is {Rng.Next(min, max)}");
        }
    }
}
