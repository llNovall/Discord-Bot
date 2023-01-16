using Discord_Bot.Database;
using Discord_Bot.Services.DataClasses;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class GuildManager
    {
        private DatabaseManager _databaseManager;
        private ILogger<BaseDiscordClient> _logger;
        private Dictionary<ulong, GuildData> _guildData = new();
        private EventId _eventId = new EventId(701, "Guild Manager");

        public bool IsReady;

        public GuildManager(DatabaseManager databaseManager, ILogger<BaseDiscordClient> logger)
        {
            _databaseManager = databaseManager;
            _logger = logger;
        }

        #region Channel

        public DiscordChannel GetChannelFor(string channel_usage_type, CommandContext ctx)
        {
            if (string.IsNullOrEmpty(channel_usage_type))
            {
                _logger.LogError(_eventId, "Channel Usage Type is null or empty.");
                return null;
            }

            if (!_guildData.ContainsKey(ctx.Guild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {ctx.Guild.Name}");
                return null;
            }

            GuildData data = _guildData[ctx.Guild.Id];

            return !data.ChannelsDict.ContainsKey(channel_usage_type) ? null : ctx.Guild.GetChannel(data.ChannelsDict[channel_usage_type]);
        }

        public DiscordChannel GetChannelFor(string channel_usage_type, DiscordGuild discordGuild)
        {
            if (string.IsNullOrEmpty(channel_usage_type))
            {
                _logger.LogError(_eventId, "Channel Usage Type is null or empty.");
                return null;
            }

            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return null;
            }

            GuildData data = _guildData[discordGuild.Id];
            return !data.ChannelsDict.ContainsKey(channel_usage_type) ? null : discordGuild.GetChannel(data.ChannelsDict[channel_usage_type]);
        }

        public async Task<bool> UpdateChannelUsageForChannel(DiscordGuild discordGuild, DiscordChannel discordChannel, string channelUsage)
        {
            if (string.IsNullOrEmpty(channelUsage))
            {
                _logger.LogError(_eventId, "Channel Usage Type is null or empty.");
                return false;
            }

            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return false;
            }

            GuildData guildDatadata = _guildData[discordGuild.Id];

            if (guildDatadata.ChannelsDict.ContainsKey(channelUsage))
                guildDatadata.ChannelsDict[channelUsage] = discordChannel.Id;
            else
                guildDatadata.ChannelsDict.Add(channelUsage, discordChannel.Id);

            return await _databaseManager.UpdateChannelUsageForChannel(discordGuild.Id, discordChannel.Id, channelUsage);
        }

        #endregion Channel

        #region Logger

        public bool GetLoggerStatusForGuild(DiscordGuild discordGuild)
        {
            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return false;
            }

            GuildData guildData = _guildData[discordGuild.Id];

            return guildData.IsLoggerEnabled;
        }

        public async Task<bool> UpdateLoggerStatusForGuild(DiscordGuild discordGuild, bool isEnabled)
        {
            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return false;
            }

            GuildData guildData = _guildData[discordGuild.Id];

            guildData.IsLoggerEnabled = isEnabled;

            return await _databaseManager.UpdateLoggerStatusForGuild(discordGuild.Id, isEnabled);
        }

        #endregion Logger

        #region Role

        public DiscordRole GetRoleFor(DiscordGuild discordGuild, string roleUsage)
        {
            if (string.IsNullOrEmpty(roleUsage))
            {
                _logger.LogError(_eventId, $"RoleUsage is null or empty.");
                return null;
            }

            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return null;
            }

            GuildData guildData = _guildData[discordGuild.Id];

            if (guildData.RolesDict.ContainsKey(roleUsage))
                return discordGuild.GetRole(guildData.RolesDict[roleUsage]);

            return null;
        }

        public Dictionary<string, DiscordRole> GetRolesFor(DiscordGuild discordGuild)
        {
            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return null;
            }

            GuildData guildData = _guildData[discordGuild.Id];
            Dictionary<string, DiscordRole> rolesDict = new Dictionary<string, DiscordRole>();

            foreach (var role in guildData.RolesDict)
            {
                DiscordRole discordRole = discordGuild.GetRole(role.Value);

                if (discordRole != null)
                {
                    rolesDict.Add(role.Key, discordRole);
                }
            }

            return rolesDict;
        }

        public async Task<bool> UpdateRoleFor(DiscordGuild discordGuild, DiscordRole discordRole, string roleUsage)
        {
            if (string.IsNullOrEmpty(roleUsage))
            {
                _logger.LogError(_eventId, $"RoleUsage is null or empty.");
                return false;
            }

            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return false;
            }

            GuildData guildData = _guildData[discordGuild.Id];

            if (guildData.RolesDict.ContainsKey(roleUsage))
                guildData.RolesDict[roleUsage] = discordRole.Id;
            else
                guildData.RolesDict.Add(roleUsage, discordRole.Id);

            return await _databaseManager.UpdateRoleForGuild(discordGuild.Id, discordRole.Id, roleUsage);
        }

        #endregion Role

        public async Task LoadGuildDataFromDatabase(IEnumerable<DiscordGuild> discordGuilds)
        {
            foreach (var item in discordGuilds)
            {
                if (!_guildData.ContainsKey(item.Id))
                    _guildData.Add(item.Id, await GetGuildDataForGuildFromDatabase(item));
            }

            IsReady = true;
        }

        public async Task<GuildData> GetGuildDataForGuildFromDatabase(DiscordGuild discordGuild)
        {
            bool isLoggerEnabled = await _databaseManager.GetLoggerStatusForGuild(discordGuild.Id);

            GuildData guildData = new()
            {
                IsLoggerEnabled = isLoggerEnabled,
                ChannelsDict = await _databaseManager.GetChannelUsageDataForGuild(discordGuild.Id),
                RolesDict = await _databaseManager.GetRolesForGuild(discordGuild.Id)
            };

            return guildData;
        }
    }
}