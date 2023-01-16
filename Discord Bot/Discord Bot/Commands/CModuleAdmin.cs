using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    [RequireUserPermissions(Permissions.Administrator)]
    internal class CModuleAdmin : BaseCommandModule
    {
        public GuildManager GuildManager;
        public DiscordClient DiscordClient;
        public Helper Helper;

        public CModuleAdmin()
        {
            InitializeAsync().GetAwaiter();
        }

        private async Task InitializeAsync()
        {
            await Task.Run(() => { while (DiscordClient == null) { } });

            DiscordClient.ComponentInteractionCreated += ComponentInteractionCreated;
        }

        private async Task ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (e.Id == "rules")
            {
                e.Handled = true;
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var discordMember = await e.Interaction.Guild.GetMemberAsync(e.Interaction.User.Id);

                if (discordMember == null)
                    return;

                var roleToAssign = GuildManager.GetRoleFor(e.Guild, "rules");

                if (roleToAssign == null)
                    return;
                try
                {
                    await discordMember.GrantRoleAsync(roleToAssign);
                }
                catch (Exception ex)
                {
                    var adminChannel = GuildManager.GetChannelFor("admin", e.Guild);

                    if (adminChannel != null)
                        await adminChannel.SendMessageAsync($"Attempted to give role {roleToAssign.Name} to {discordMember.Mention} but failed.\nReason : {ex.Message}");
                }
            }
        }

        [Command("setch")]
        public async Task SetChannelForAsync(CommandContext ctx, DiscordChannel channel, string channelUsage)
        {
            var adminChannel = GuildManager.GetChannelFor("admin", ctx);

            if (adminChannel == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to find admin channel.");
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
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);

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

            bool isUpdated = await GuildManager.UpdateChannelUsageForChannel(ctx.Guild, discordChannel, "music");

            if (!isUpdated)
            {
                await adminChannel.SendMessageAsync("Failed to save music channel to the database.");
                return;
            }

            DiscordChannel musicChannel = GuildManager.GetChannelFor(channel_usage_type: "music", ctx);
            if (musicChannel == null)
                await adminChannel.SendMessageAsync($"Failed to set {discordChannel.Mention} as music channel.");
            else
                await adminChannel.SendMessageAsync($"Successfully set {discordChannel.Mention} as music channel.");
        }

        [Command("rules")]
        public async Task PostRulesAsync(CommandContext ctx, DiscordRole roleToAssign, DiscordEmoji reactionEmoji, DiscordChannel rulesChannel, string title, string image_uri, [RemainingText] string rulesText)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);

            if (adminChannel == null)
            {
                await Helper.SendMessageToChannelAsync(DiscordClient, ctx.Channel, Helper.MessageSeverity.Negative, "Failed to find admin channel.");
                return;
            }

            if (rulesChannel == null || roleToAssign == null || reactionEmoji == null || string.IsNullOrEmpty(rulesText) || string.IsNullOrEmpty(title))
                return;

            var embed = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithDescription(rulesText)
                .WithAuthor(name: ctx.Client.CurrentUser.Username, url: ctx.Client.CurrentUser.AvatarUrl, iconUrl: ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(DiscordColor.Red)
                .WithImageUrl(image_uri);

            var messageBuilder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "rules", "Accept", emoji: new DiscordComponentEmoji(reactionEmoji)));

            bool isSuccess = await GuildManager.UpdateRoleFor(ctx.Guild, roleToAssign, "rules");

            if (!isSuccess)
            {
                await adminChannel.SendMessageAsync("Failed to save to database.");
                return;
            }

            await rulesChannel.SendMessageAsync(messageBuilder);
        }
    }
}