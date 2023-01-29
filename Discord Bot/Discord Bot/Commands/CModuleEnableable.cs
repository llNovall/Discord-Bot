using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    public abstract class CModuleEnableable : BaseCommandModule
    {
        public GuildManager GuildManager;
        public DiscordClient DiscordClient;
        public Helper Helper;

        protected Dictionary<ulong, bool> _status = new();
        protected string _serviceName;

        protected bool IsServiceEnabled(ulong guildId) => _status.ContainsKey(guildId) ? _status[guildId] : false;

        protected async Task Initialize()
        {
            await Task.Run(() =>
            {
                while (DiscordClient == null)
                {
                }
            });

            var guilds = DiscordClient.Guilds;

            await Task.Run(() =>
            {
                while (!GuildManager.IsReady)
                {
                }
            });

            await Task.Run(() =>
            {
                foreach (var item in guilds)
                {
                    _status.Add(item.Key, GuildManager.GetStatusForServiceForGuild(item.Value, _serviceName));
                }
            });
        }

        protected async Task EnableModuleAsync(CommandContext ctx, DiscordChannel channelToAssign = null)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);

            if (adminChannel == null)
                return;

            if (!await GuildManager.UpdateStatusForServiceForGuild(ctx.Guild, _serviceName, true))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Failed to update {_serviceName} status.");
                return;
            }

            if (channelToAssign != null)
                if (!await GuildManager.UpdateChannelUsageForChannel(ctx.Guild, channelToAssign, _serviceName))
                {
                    await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Failed to set channel for {_serviceName}.");
                    return;
                }

            if (_status.ContainsKey(ctx.Guild.Id))
                _status[ctx.Guild.Id] = true;
            else
                _status.Add(ctx.Guild.Id, true);

            await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Positive, $"Enabled {_serviceName}.");
        }

        public async Task DisableModuleAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);

            if (adminChannel == null)
                return;

            await GuildManager.UpdateStatusForServiceForGuild(ctx.Guild, _serviceName, false);

            if (_status.ContainsKey(ctx.Guild.Id))
                _status[ctx.Guild.Id] = false;
            else
                _status.Add(ctx.Guild.Id, false);

            await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Positive, $"Disabled {_serviceName}.");
        }
    }
}