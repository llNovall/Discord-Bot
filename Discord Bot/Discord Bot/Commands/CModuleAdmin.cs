using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    [RequireUserPermissions(Permissions.Administrator)]
    internal class CModuleAdmin : BaseCommandModule
    {
        public GuildManager GuildManager;
        public DiscordClient DiscordClient;
        public Helper Helper;

        [Command("ch")]
        public async Task GetChannelsForGuildAsync(CommandContext ctx)
        {
            var adminChannel = GuildManager.GetChannelFor("admin", ctx);

            if (adminChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel,
                    Helper.MessageSeverity.Negative, "I've failed to find admin channel.");
                return;
            }

            var channelsDict = GuildManager.GetChannelsForGuild(ctx.Guild);

            if (channelsDict == null)
                return;

            string content = "";
            int iter = 1;
            foreach (var item in channelsDict)
            {
                DiscordChannel channel = ctx.Guild.GetChannel(item.Value);
                if (channel == null)
                    continue;

                content += $"{iter}) {item.Key} - {channel.Mention}\n";
                iter += 1;
            }

            await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel,
                Helper.MessageSeverity.Positive, "Channels", content);
        }

        [Command("setch")]
        public async Task SetChannelForAsync(CommandContext ctx, DiscordChannel channel, string channelUsage)
        {
            var adminChannel = GuildManager.GetChannelFor("admin", ctx);

            if (adminChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel,
                    Helper.MessageSeverity.Negative, "I've failed to find admin channel.");
                return;
            }

            if (channel == null || string.IsNullOrEmpty(channelUsage))
                return;

            var isSuccess = await GuildManager.UpdateChannelUsageForChannel(ctx.Guild, channel, channelUsage);

            if (!isSuccess)
                await adminChannel.SendMessageAsync("Failed to save to database.");

            await adminChannel.SendMessageAsync($"{channel.Mention} set as channel for {channelUsage}.");
        }

        [Command("ping")]
        public async Task PingAsync(CommandContext ctx)
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = $"Ping : {ctx.Client.Ping}",
                Color = DiscordColor.Green
            };

            await ctx.RespondAsync(embed);
        }

        [Command("setadminchannel")]
        public async Task SetAdminChannelAsync(CommandContext ctx, DiscordChannel discordChannel)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            DiscordChannel channelToSendMessage = adminChannel ?? ctx.Channel;

            if (discordChannel == null)
            {
                await channelToSendMessage.SendMessageAsync("Pass in the channel to be used for admin as parameter.");
                return;
            }

            bool isUpdated = await GuildManager.UpdateChannelUsageForChannel(ctx.Guild, discordChannel, "admin");

            if (!isUpdated)
            {
                await channelToSendMessage.SendMessageAsync("Failed to save admin channel to the database.");
                return;
            }

            adminChannel = GuildManager.GetChannelFor("admin", ctx);

            channelToSendMessage = adminChannel;
            await channelToSendMessage.SendMessageAsync($"Successfully set {channelToSendMessage.Mention} as admin channel.");
        }

        [Command("setmusicchannel")]
        public async Task SetMusicChannelAsync(CommandContext ctx, DiscordChannel discordChannel)
        {
            var adminChannel = GuildManager.GetChannelFor("admin", ctx);

            if (adminChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel,
                    Helper.MessageSeverity.Negative, "I've failed to find admin channel.");
                return;
            }

            if (discordChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel,
                    Helper.MessageSeverity.Negative, "Pass in the channel to be used for music as parameter.");
                return;
            }

            bool isUpdated = await GuildManager.UpdateChannelUsageForChannel(ctx.Guild, discordChannel, "music");

            if (!isUpdated)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel,
                    Helper.MessageSeverity.Negative, "Failed to save music channel to the database.");
                return;
            }

            DiscordChannel musicChannel = GuildManager.GetChannelFor(channelUsageType: "music", ctx);
            if (musicChannel == null)
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel,
                    Helper.MessageSeverity.Negative, "Failed",
                    $"Failed to set {discordChannel.Mention} as music channel.");
            else
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel,
                    Helper.MessageSeverity.Negative, "Failed",
                    $"Successfully set {discordChannel.Mention} as music channel.");
        }
    }
}