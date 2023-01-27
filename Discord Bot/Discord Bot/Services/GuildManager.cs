using Discord_Bot.Database;
using Discord_Bot.Services.DataClasses;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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

        public Dictionary<string, ulong> GetChannelsForGuild(DiscordGuild discordGuild)
        {
            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return null;
            }

            return _guildData[discordGuild.Id].ChannelsDict;
        }

        public DiscordChannel GetChannelFor(string channelUsageType, InteractionContext ctx)
        {
            if (string.IsNullOrEmpty(channelUsageType))
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

            return !data.ChannelsDict.ContainsKey(channelUsageType) ? null : ctx.Guild.GetChannel(data.ChannelsDict[channelUsageType]);
        }

        public DiscordChannel GetChannelFor(string channelUsageType, CommandContext ctx)
        {
            if (string.IsNullOrEmpty(channelUsageType))
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

            return !data.ChannelsDict.ContainsKey(channelUsageType) ? null : ctx.Guild.GetChannel(data.ChannelsDict[channelUsageType]);
        }

        public DiscordChannel GetChannelFor(string channelUsageType, DiscordGuild discordGuild)
        {
            if (string.IsNullOrEmpty(channelUsageType))
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
            return !data.ChannelsDict.ContainsKey(channelUsageType) ? null : discordGuild.GetChannel(data.ChannelsDict[channelUsageType]);
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

        #region ServiceStatus

        public bool GetStatusForServiceForGuild(DiscordGuild discordGuild, string serviceName)
        {
            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return false;
            }

            GuildData guildData = _guildData[discordGuild.Id];

            return guildData.ServiceStatusDict.ContainsKey(serviceName) ? guildData.ServiceStatusDict[serviceName] : false;
        }

        public async Task<bool> UpdateStatusForServiceForGuild(DiscordGuild discordGuild, string serviceName, bool isEnabled)
        {
            if (!_guildData.ContainsKey(discordGuild.Id))
            {
                _logger.LogError(_eventId, $"Failed to find GuildData for guild {discordGuild.Name}");
                return false;
            }

            GuildData guildData = _guildData[discordGuild.Id];

            if (guildData.ServiceStatusDict.ContainsKey(serviceName))
                guildData.ServiceStatusDict[serviceName] = isEnabled;
            else
                guildData.ServiceStatusDict.Add(serviceName, isEnabled);

            return await _databaseManager.UpdateStatusForServiceForGuild(discordGuild.Id, serviceName, isEnabled);
        }

        #endregion ServiceStatus

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
            GuildData guildData = new()
            {
                ChannelsDict = await _databaseManager.GetChannelUsageDataForGuild(discordGuild.Id),
                RolesDict = await _databaseManager.GetRolesForGuild(discordGuild.Id),
                ServiceStatusDict = await _databaseManager.GetServiceStatusForGuild(discordGuild.Id)
            };

            return guildData;
        }
    }
}