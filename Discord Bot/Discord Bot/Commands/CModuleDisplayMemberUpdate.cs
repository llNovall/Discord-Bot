using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Discord_Bot.Commands
{
    [RequireUserPermissions(Permissions.Administrator)]
    [Category("Member Display")]
    public class CModuleDisplayMemberUpdate : CModuleEnableable
    {
        private Dictionary<DiscordGuild, List<DiscordMember>> _membersDict = new();

        private System.Timers.Timer _timer;

        public CModuleDisplayMemberUpdate()
        {
            this.
            _serviceName = "member_display";

            Initialize().GetAwaiter();
            InitializeModuleAsync().GetAwaiter();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Task.Run(async () =>
            {
                foreach (var item in _membersDict)
                {
                    DiscordGuild guild = item.Key;

                    if (IsServiceEnabled(guild.Id))
                    {
                        await UpdateTotalOnlineMemberCount(guild);
                    }
                }
            });
        }

        private async Task InitializeModuleAsync()
        {
            await Task.Run(() =>
            {
                while (DiscordClient == null)
                {
                }
            });

            DiscordClient.GuildMemberAdded += DiscordClient_GuildMemberAdded;
            DiscordClient.GuildMemberRemoved += DiscordClient_GuildMemberRemoved;
            DiscordClient.GuildUnavailable += DiscordClient_GuildUnavailable;
            DiscordClient.GuildAvailable += DiscordClient_GuildAvailable;

            var guilds = DiscordClient.Guilds;

            foreach (var guild in guilds)
                _membersDict.Add(guild.Value, guild.Value.Members.Values.ToList());

            _timer = new(TimeSpan.FromSeconds(300));
            _timer.AutoReset = true;
            _timer.Elapsed += Timer_Elapsed;
            _timer.Enabled = true;
        }

        private async Task DiscordClient_GuildAvailable(DiscordClient sender, DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            if (!_membersDict.ContainsKey(e.Guild))
                _membersDict.Add(e.Guild, e.Guild.Members.Values.ToList());
        }

        private async Task DiscordClient_GuildUnavailable(DiscordClient sender, DSharpPlus.EventArgs.GuildDeleteEventArgs e)
        {
            if (_membersDict.ContainsKey(e.Guild))
                _membersDict.Remove(e.Guild);
        }

        private async Task UpdateTotalOnlineMemberCount(DiscordGuild discordGuild)
        {
            DiscordChannel totalOnlineMemberChannel = GuildManager.GetChannelFor("total_online_members", discordGuild);

            if (totalOnlineMemberChannel == null)
                return;

            if (_membersDict.ContainsKey(discordGuild))
            {
                int count = _membersDict[discordGuild].Count(c =>
                    c.Presence?.Status is UserStatus.Online or UserStatus.Idle or UserStatus.DoNotDisturb);

                if (totalOnlineMemberChannel.Name != $"Online Members : {count}")
                    await totalOnlineMemberChannel.ModifyAsync(model => model.Name = $"Online Members : {count}");
            }
        }

        private async Task UpdateTotalMemberCount(DiscordGuild discordGuild)
        {
            _membersDict[discordGuild] = discordGuild.Members.Values.ToList();

            DiscordChannel totalMemberChannel = GuildManager.GetChannelFor("total_members", discordGuild);

            if (totalMemberChannel == null)
                return;

            await totalMemberChannel.ModifyAsync(new Action<ChannelEditModel>(model => model.Name = $"Members : {discordGuild.MemberCount}"));
        }

        private async Task DiscordClient_GuildMemberAdded(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            if (IsServiceEnabled(e.Guild.Id))
                await UpdateTotalMemberCount(e.Guild);
        }

        private async Task DiscordClient_GuildMemberRemoved(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.GuildMemberRemoveEventArgs e)
        {
            if (IsServiceEnabled(e.Guild.Id))
                await UpdateTotalMemberCount(e.Guild);
        }

        [Command("enablememberdisplay")]
        [Description("Enable member display module.\nUsage - **[Prefix]enablememberdisplay**")]
        public async Task EnableMemberDisplay(CommandContext ctx)
        {
            await EnableModuleAsync(ctx);
        }

        [Command("disablememberdisplay")]
        [Description("Disable member display module.\nUsage - **[Prefix]disablememberdisplay**")]
        public async Task DisableMemberDisplay(CommandContext ctx)
        {
            await DisableModuleAsync(ctx);
        }

        [Command("totalmembers")]
        [Description("Enabled updating the name of provided channel with total number of members in the server.\nUsage - **[Prefix]totalmembers [Channel]**")]
        public async Task DisplayTotalMembers(CommandContext ctx, DiscordChannel totalChannel)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);

            if (adminChannel == null)
                return;

            if (IsServiceEnabled(ctx.Guild.Id))
            {
                bool isSuccess = await GuildManager.UpdateChannelUsageForChannel(ctx.Guild, totalChannel, "total_members");

                if (isSuccess)
                {
                    await Helper.SendMessageToChannelAsync(DiscordClient, adminChannel, Helper.MessageSeverity.Positive,
                        "Enabled updating total members count", $"{totalChannel} successfully set.");

                    await UpdateTotalMemberCount(ctx.Guild);
                }
                else
                    await Helper.SendMessageToChannelAsync(DiscordClient, adminChannel, Helper.MessageSeverity.Negative,
                        "Failed to enable updating total members count", $"{totalChannel} failed to set.");

                return;
            }

            await Helper.SendMessageToChannelAsync(DiscordClient, adminChannel, Helper.MessageSeverity.Negative,
                "Enable this service by using the command **enablememberdisplay**.");
        }

        [Command("totalonlinemembers")]
        [Description("Enabled updating the name of provided channel with total number of online members in the server.\nUsage - **[Prefix]totalonlinemembers [Channel]**")]
        public async Task DisplayTotalOnlineMembers(CommandContext ctx, DiscordChannel totalChannel)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);

            if (adminChannel == null)
                return;

            if (IsServiceEnabled(ctx.Guild.Id))
            {
                bool isSuccess = await GuildManager.UpdateChannelUsageForChannel(ctx.Guild, totalChannel, "total_online_members");

                if (isSuccess)
                {
                    await Helper.SendMessageToChannelAsync(DiscordClient, adminChannel, Helper.MessageSeverity.Positive,
                        "Enabled updating total online members count", $"{totalChannel} successfully set.");

                    await UpdateTotalOnlineMemberCount(ctx.Guild);
                }
                else
                    await Helper.SendMessageToChannelAsync(DiscordClient, adminChannel, Helper.MessageSeverity.Negative,
                        "Failed to enable updating total online members count", $"{totalChannel} failed to set.");

                return;
            }

            await Helper.SendMessageToChannelAsync(DiscordClient, adminChannel, Helper.MessageSeverity.Negative,
                "Enable this service by using the command **enablememberdisplay**.");
        }
    }
}