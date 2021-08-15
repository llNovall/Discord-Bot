using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Tiny_Bot.Commands
{
    class CModuleAdmin : BaseCommandModule
    {
        //[Command("prefix")]
        //public async Task ChangePrefix(CommandContext ctx, char prefix)
        //{
        //    CommandsNextExtension extension = ctx.Client.GetCommandsNext();
        //    extension.Client.
        //}

        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = $"Ping : { ctx.Client.Ping}",
                Color = DiscordColor.Green
            };

            await ctx.RespondAsync(embed);
        }
    }
}
