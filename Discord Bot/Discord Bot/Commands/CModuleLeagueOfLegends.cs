using Discord_Bot.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using Discord_Bot.Services.DataClasses;

namespace Discord_Bot.Commands
{
    internal class CModuleLeagueOfLegends : BaseCommandModule
    {
        public WhatIsMyMMRService MMRService;
        public DiscordEmbedBuilderHelper Embed;

        [Command("arammmr")]
        public async Task FindAramMMR(CommandContext ctx, string region, [RemainingText] string userName)
        {
            if (!string.IsNullOrEmpty(region) && !string.IsNullOrEmpty(userName))
            {
                WhatISMyMMRData data = await MMRService.FindMMRFor(region, userName);

                if (data != null)
                {
                    DiscordEmbedBuilder embed = Embed.GetDefaultEmbedTemplate(ctx.Client);

                    embed.WithTitle($"ARAM MMR for *{userName}* in *{region.ToUpper()}*");
                    embed.AddField($"Current MMR", $"{data.ARAM.Avg} ±{data.ARAM.Err}");
                    embed.AddField($"Closest Rank", $"{data.ARAM.ClosestRank}");
                    embed.WithFooter($"Powered by whatismymmr.com");

                    await ctx.RespondAsync(embed);
                }
                else

                {
                    DiscordEmbedBuilder embed = Embed.GetDefaultEmbedTemplate(ctx.Client);
                    embed.WithTitle($":no_entry: Give me proper values to search. :no_entry:");
                    await ctx.RespondAsync(embed);
                }
            }
            else
            {
                await ctx.RespondAsync($"Failed to find MMR for {userName} in region {region}.");
            }
        }
    }
}