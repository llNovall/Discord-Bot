using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tiny_Bot.DataClasses;
using Tiny_Bot.Utility;

namespace Tiny_Bot.Commands
{
    class CModuleGreet : BaseCommandModule
    {
        public DiscordEmbedBuilderHelper EmbedHelper;
        public Random Random;
        public GIFTenorService GIFTenorService;

        [Command("greet")]
        public async Task Greet(CommandContext ctx)
        {
            string url = await GIFTenorService.GetGIFUrl("animewave", 25);

            if (!string.IsNullOrEmpty(url))
            {
                await ctx.RespondAsync($"Hello, {ctx.Member.Mention}!");
                await ctx.Channel.SendMessageAsync(url);
            }
            else
                Console.WriteLine($"Failed to load GIF.");
        }

        [Command("greet")]
        [Cooldown(1,3,CooldownBucketType.Guild)]
        public async Task Greet(CommandContext ctx, DiscordUser discordUser)
        {
            string url = "";

            if(ctx.Client.CurrentUser == discordUser)
            {
                url = await GIFTenorService.GetGIFUrl("animewave", 25);

                if (!string.IsNullOrEmpty(url))
                {
                    await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} greeted {discordUser.Mention}.");
                    await ctx.Channel.SendMessageAsync(url);
                    await Greet(ctx);
                }
                else
                    Console.WriteLine($"Failed to load GIF.");

            }
            else if (ctx.Member == discordUser)
            {
                url = await GIFTenorService.GetGIFUrl("anime-confused", 25);
                if (!string.IsNullOrEmpty(url))
                {
                    await ctx.RespondAsync($"{ctx.Member.Mention} greets.. who? What? Why?");
                    await ctx.Channel.SendMessageAsync(url);
                }
                else
                    Console.WriteLine($"Failed to load GIF.");
            }
            else
            {
                url = await GIFTenorService.GetGIFUrl("animewave", 25);
                if (!string.IsNullOrEmpty(url))
                {
                    await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} greeted {discordUser.Mention}.");
                    await ctx.Channel.SendMessageAsync(url);
                }
                else
                    Console.WriteLine($"Failed to load GIF.");
            }

            
        }

        [Command("cat")]
        public async Task GetACatGIF(CommandContext ctx)
        {
            string url = await GIFTenorService.GetGIFUrl("kitty", 25);

            if (!string.IsNullOrEmpty(url))
            {
                await ctx.RespondAsync(url);
            }
            else
                Console.WriteLine($"Failed to load GIF.");
            
        }

        [Command("dog")]
        public async Task GetADogGIF(CommandContext ctx)
        {
            string url = await GIFTenorService.GetGIFUrl("dog", 25);

            if (!string.IsNullOrEmpty(url))
            {
                await ctx.RespondAsync(url);
            }
            else
                Console.WriteLine($"Failed to load GIF.");
        }
    }
}
