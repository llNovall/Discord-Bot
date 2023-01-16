using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class Helper
    {
        public enum MessageSeverity
        {
            Positive,
            Neutral,
            Negative
        }

        private GuildManager _guildManager;

        public Helper(GuildManager guildManager)
        {
            _guildManager = guildManager;
        }

        public async Task<DiscordChannel> GetAdminChannel(CommandContext ctx)
        {
            DiscordChannel adminChannel = _guildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
            {
                await SendMessageToChannelAsync(ctx.Client, ctx.Channel, MessageSeverity.Negative, "Failed to find admin channel.");
            }

            return adminChannel;
        }

        public async Task SendMessageToChannelAsync(BaseDiscordClient discordClient, DiscordChannel channelToSendMessage, MessageSeverity messageSeverity, string messageToSend)
        {
            if (channelToSendMessage == null || discordClient == null || string.IsNullOrWhiteSpace(messageToSend))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(name: discordClient.CurrentUser.Username, url: discordClient.CurrentUser.AvatarUrl, iconUrl: discordClient.CurrentUser.AvatarUrl)
                .WithColor(messageSeverity switch
                {
                    MessageSeverity.Positive => DiscordColor.Green,
                    MessageSeverity.Negative => DiscordColor.Red,
                    _ => DiscordColor.White
                });

            embed.WithTitle(messageToSend);

            await channelToSendMessage.SendMessageAsync(embed);
        }

        public static async Task SendMessageToChannelAsync(BaseDiscordClient discordClient, DiscordChannel channelToSendMessage, MessageSeverity messageSeverity, string title, string messageToSend)
        {
            if (channelToSendMessage == null || discordClient == null || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(messageToSend))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(name: discordClient.CurrentUser.Username, url: discordClient.CurrentUser.AvatarUrl, iconUrl: discordClient.CurrentUser.AvatarUrl)
                .WithColor(messageSeverity switch
                {
                    MessageSeverity.Positive => DiscordColor.Green,
                    MessageSeverity.Negative => DiscordColor.Red,
                    _ => DiscordColor.White
                });

            embed.WithTitle(title)
                .WithDescription(messageToSend);

            await channelToSendMessage.SendMessageAsync(embed);
        }

        public static DiscordEmbedBuilder CreateEmbedBuilder(DiscordMember discordMember, DiscordColor color)
            => new DiscordEmbedBuilder()
                .WithAuthor(discordMember.Nickname, discordMember.AvatarUrl, discordMember.AvatarUrl)
                .WithThumbnail(discordMember.AvatarUrl, 75, 75)
                .WithColor(color)
                .WithTimestamp(DateTime.Now);

        public static DiscordEmbedBuilder CreateEmbedBuilder(DiscordUser discordUser, DiscordColor color)
            => new DiscordEmbedBuilder()
                .WithAuthor(discordUser.Username, discordUser.AvatarUrl, discordUser.AvatarUrl)
                .WithThumbnail(discordUser.AvatarUrl, 75, 75)
                .WithColor(color)
                .WithTimestamp(DateTime.Now);

        public static DiscordEmbedBuilder CreateEmbedBuilder(DiscordColor color)
            => new DiscordEmbedBuilder()
                .WithColor(color)
                .WithTimestamp(DateTime.Now);
    }
}