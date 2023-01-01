using Discord_Bot.Database;
using Discord_Bot.Services;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    internal class CModuleAdmin : BaseCommandModule
    {
        public DatabaseManager DatabaseManager;
        public ChannelFinder ChannelFinder;

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
                Title = $"Ping : {ctx.Client.Ping}",
                Color = DiscordColor.Green
            };

            await ctx.RespondAsync(embed);
        }

        [Command("setadminchannel"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task SetAdminChannel(CommandContext ctx, DiscordChannel discordChannel)
        {
            DiscordChannel adminChannel = await ChannelFinder.GetChannelFor("admin", ctx);
            DiscordChannel channelToSendMessage = adminChannel != null ? adminChannel : ctx.Channel;

            if (discordChannel == null)
            {
                await channelToSendMessage.SendMessageAsync("Pass in the channel to be used for admin as parameter.");
                return;
            }

            bool isUpdated = await DatabaseManager.UpdateChannelUsageForChannel(guildId: ctx.Guild.Id, channelId: discordChannel.Id, channel_usage_type: "admin");

            if (!isUpdated)
            {
                await channelToSendMessage.SendMessageAsync("Failed to save admin channel to the database.");
                return;
            }

            adminChannel = await ChannelFinder.GetChannelFor("admin", ctx);

            if (adminChannel == null)
            {
                await channelToSendMessage.SendMessageAsync("Failed to retrieve admin channel from the database.");
                return;
            }

            channelToSendMessage = adminChannel;
            await channelToSendMessage.SendMessageAsync($"Successfully set {channelToSendMessage.Mention} as admin channel.");
        }

        [Command("setmusicchannel"), RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task SetMusicChannel(CommandContext ctx, DiscordChannel discordChannel)
        {
            if (discordChannel == null)
                return;

            DiscordChannel adminChannel = await ChannelFinder.GetChannelFor("admin", ctx);

            if (adminChannel == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to find admin channel.");
                return;
            }

            if (discordChannel == null)
            {
                await adminChannel.SendMessageAsync("Pass in the channel to be used for music as parameter.");
                return;
            }

            bool isUpdated = await DatabaseManager.UpdateChannelUsageForChannel(guildId: ctx.Guild.Id, channelId: discordChannel.Id, channel_usage_type: "music");

            if (!isUpdated)
            {
                await adminChannel.SendMessageAsync("Failed to save music channel to the database.");
                return;
            }

            GuildChannelUsageData data = await DatabaseManager.GetGuildChannelUsageData(guildId: ctx.Guild.Id, channel_usage_type: "music");

            if (data.ChannelUsageType == null)
            {
                await adminChannel.SendMessageAsync("Failed to retrieve music channel from the database.");
                return;
            }

            DiscordChannel musicChannel = await ChannelFinder.GetChannelFor(channel_usage_type: "music", ctx);
            if (musicChannel == null)
                await adminChannel.SendMessageAsync($"Failed to set {discordChannel.Mention} as music channel.");
            else
                await adminChannel.SendMessageAsync($"Successfully set {discordChannel.Mention} as music channel.");
        }
    }
}