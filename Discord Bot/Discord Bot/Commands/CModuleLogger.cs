using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    [RequireUserPermissions(Permissions.Administrator)]
    internal class CModuleLogger : CModuleEnableable
    {
        public CModuleLogger()
        {
            _serviceName = "log";

            base.Initialize().GetAwaiter();

            InitializeLoggerAsync().GetAwaiter();
        }

        private async Task InitializeLoggerAsync()
        {
            await Task.Run(() =>
            {
                while (DiscordClient == null)
                {
                }
            });

            DiscordClient.GuildMemberUpdated += DiscordClient_GuildMemberUpdated;
            DiscordClient.GuildMemberAdded += DiscordClient_GuildMemberAdded;
            DiscordClient.GuildMemberRemoved += DiscordClient_GuildMemberRemoved;
            DiscordClient.GuildRoleCreated += DiscordClient_GuildRoleCreated;
            DiscordClient.GuildRoleDeleted += DiscordClient_GuildRoleDeleted;
            //DiscordClient.GuildRoleUpdated += DiscordClient_GuildRoleUpdated;
            DiscordClient.InviteCreated += DiscordClient_InviteCreated;
            DiscordClient.InviteDeleted += DiscordClient_InviteDeleted;
            DiscordClient.ChannelCreated += DiscordClient_ChannelCreated;
            DiscordClient.ChannelDeleted += DiscordClient_ChannelDeleted;
            DiscordClient.MessageDeleted += DiscordClient_MessageDeleted;
            DiscordClient.MessageUpdated += DiscordClient_MessageUpdated;
            //DiscordClient.VoiceStateUpdated += DiscordClient_VoiceStateUpdated;
        }

        [Command("enablelog")]
        public async Task EnableLogAsync(CommandContext ctx, DiscordChannel logChannel)
        {
            await EnableModuleAsync(ctx, logChannel);
        }

        [Command("disablelog")]
        public async Task DisableLogAsync(CommandContext ctx)
        {
            await DisableModuleAsync(ctx);
        }

        private async Task DiscordClient_InviteDeleted(DiscordClient sender, InviteDeleteEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(sender.CurrentUser, DiscordColor.Lilac);

            embed.WithTitle("Invite Deleted");

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_InviteCreated(DiscordClient sender, InviteCreateEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(sender.CurrentUser, DiscordColor.Lilac);

            embed.WithTitle("Invite Created")
                .WithDescription($"{e.Invite.Inviter.Mention} created an invite with code {e.Invite.Code}.");

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_GuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(DiscordColor.Lilac);
            embed.WithTitle($"Role Deleted - {e.Role.Name}")
                .WithDescription(e.Role.Name);

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_GuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(DiscordColor.Lilac);
            embed.WithTitle($"Role Created")
                .AddField($"{e.Role.Mention}", $"{e.Role.Permissions.ToPermissionString()}");

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_GuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(e.Member, DiscordColor.Lilac);

            embed.WithTitle("Member Left")
                .WithDescription($"{e.Member.Mention} left {e.Guild.Name}.");

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_GuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(e.Member, DiscordColor.Lilac);

            embed.WithTitle("Member Joined").WithDescription($"{e.Member.Mention} has joined {e.Guild.Name}.");

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            if (e.NicknameBefore != e.NicknameAfter)
            {
                DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(e.Member, DiscordColor.Lilac);

                embed.WithTitle("Nickname Changed")
                    .WithDescription($"{e.Member.Mention}");
                if (!string.IsNullOrWhiteSpace(e.NicknameBefore))
                    embed.AddField("Previous Nickname", e.NicknameBefore);
                if (!string.IsNullOrWhiteSpace(e.NicknameAfter))
                    embed.AddField("New Nickname", e.NicknameAfter);

                await channel.SendMessageAsync(embed);
            }

            if (e.RolesBefore.Count != e.RolesAfter.Count)
            {
                DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(e.Member, DiscordColor.Lilac)
                .WithTitle($"Roles Updated")
                .WithDescription($"{e.Member.Mention}");
                string[] rolesAdded = e.RolesAfter.Where(c => !e.RolesBefore.Contains(c)).Select(c => c.Mention).ToArray();
                string[] rolesRemoved = e.RolesBefore.Where(c => !e.RolesAfter.Contains(c)).Select(c => c.Mention).ToArray();

                if (rolesAdded.Length > 0)
                    embed.AddField("Roles Added", string.Join("\n", rolesAdded));
                if (rolesRemoved.Length > 0)
                    embed.AddField("Roles Removed", string.Join("\n", rolesRemoved));

                await channel.SendMessageAsync(embed);
            }
        }

        private async Task DiscordClient_MessageUpdated(DiscordClient sender, MessageUpdateEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            if (e.MessageBefore == e.Message)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(sender.CurrentUser, DiscordColor.Lilac);

            embed.WithTitle("Message Updated")
                .AddField("Author", $"{e.Message.Author.Mention}")
                .AddField("Previous Message", $"{e.MessageBefore.Content}")
                .AddField("Updated Message", $"{e.Message.Content}")
                .AddField("Channel", $"{e.Channel.Mention}");

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_MessageDeleted(DiscordClient sender, MessageDeleteEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            if (string.IsNullOrWhiteSpace(e.Message.Content))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(sender.CurrentUser, DiscordColor.Lilac);

            embed.WithTitle("Message Deleted")
                .AddField("Author", $"{e.Message.Author.Mention}")
                .AddField("Deleted Message", $"{e.Message.Content}")
                .AddField("Channel", $"{e.Channel.Mention}");

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_ChannelDeleted(DiscordClient sender, ChannelDeleteEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(e.Guild.CurrentMember, DiscordColor.Lilac);

            embed.WithTitle("Channel Deleted")
                .WithDescription($"{e.Channel.Mention}");

            await channel.SendMessageAsync(embed);
        }

        private async Task DiscordClient_ChannelCreated(DiscordClient sender, ChannelCreateEventArgs e)
        {
            if (e.Guild == null)
                return;

            if (!IsServiceEnabled(e.Guild.Id))
                return;

            DiscordChannel channel = GuildManager.GetChannelFor("log", e.Guild);

            if (channel == null)
                return;

            DiscordEmbedBuilder embed = Helper.CreateEmbedBuilder(e.Guild.CurrentMember, DiscordColor.Lilac);

            embed.WithTitle("Channel Created")
                .WithDescription($"{e.Channel.Mention}");

            await channel.SendMessageAsync(embed);
        }
    }
}