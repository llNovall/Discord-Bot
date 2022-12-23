using Discord_Bot.Database;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    internal class ChannelFinder
    {
        public DatabaseManager DatabaseManager;

        public ChannelFinder(DatabaseManager databaseManager)
        {
            DatabaseManager= databaseManager;
        }

        public async Task<DiscordChannel> GetChannelFor(string channel_usage_type, CommandContext ctx)
        {
            if (string.IsNullOrEmpty(channel_usage_type))
                return null;

            GuildChannelUsageData data = await DatabaseManager.GetGuildChannelUsageData(guildId: ctx.Guild.Id, channel_usage_type: channel_usage_type);
            DiscordChannel foundChannel = ctx.Guild.Channels.FirstOrDefault(c => c.Value.Id == data.ChannelId).Value;

            return foundChannel;
        }
    }
}
