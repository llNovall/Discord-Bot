﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    internal class CModuleSimpleMaths : BaseCommandModule
    {
        [Command("add")]
        [Description("I can add mutiple numbers.")]
        public async Task Sum(CommandContext ctx, params int[] numbers)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(name: ctx.Client.CurrentUser.Username, url: ctx.Client.CurrentUser.AvatarUrl, iconUrl: ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(DiscordColor.CornflowerBlue);
            if (numbers.Length > 0)
            {
                embed.AddField($"The sum of {string.Join(", ", numbers)} is...", $"```fix\n{numbers.Sum()}```");
            }
            else
            {
                embed.WithTitle("```What do you want me to add here...?```");
            }

            await ctx.RespondAsync(embed);
        }

        [Command("minus")]
        [Description("I can subtract multiple numbers.")]
        public async Task Subtract(CommandContext ctx, params int[] numbers)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(name: ctx.Client.CurrentUser.Username, url: ctx.Client.CurrentUser.AvatarUrl, iconUrl: ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(DiscordColor.CornflowerBlue);

            if (numbers.Length > 0)
            {
                int result = numbers[0];

                for (int i = 1; i < numbers.Length; i++)
                    result -= numbers[i];

                embed.AddField($"The difference of {string.Join(", ", numbers)} is...", $"```fix\n{result}```");
            }
            else
            {
                embed.WithTitle("```What do you want me to subtract here...?```");
            }

            await ctx.RespondAsync(embed);
        }

        [Command("multiply")]
        [Description("I can multiply multiple numbers.")]
        public async Task Multiply(CommandContext ctx, params int[] numbers)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(name: ctx.Client.CurrentUser.Username, url: ctx.Client.CurrentUser.AvatarUrl, iconUrl: ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(DiscordColor.CornflowerBlue);
            if (numbers.Length > 0)
            {
                int result = 1;
                numbers.ToList().ForEach(c => result *= c);

                embed.AddField($"The product of {string.Join(", ", numbers)} is...", $"```fix\n{result}```");
            }
            else
            {
                embed.WithTitle("```What do you want me to multiply here...?```");
            }

            await ctx.RespondAsync(embed);
        }
    }
}