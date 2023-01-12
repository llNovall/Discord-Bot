using Discord_Bot.Database;
using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    internal class CModuleAdmin : BaseCommandModule
    {
        public DatabaseManager DatabaseManager;
        public ChannelFinder ChannelFinder;
        public DiscordClient DiscordClient;
        public RolesFinder RolesFinder;
        //[Command("prefix")]
        //public async Task ChangePrefix(CommandContext ctx, char prefix)
        //{
        //    CommandsNextExtension extension = ctx.Client.GetCommandsNext();
        //    extension.Client.
        //}

        public CModuleAdmin()
        {
            Initialize().GetAwaiter();
        }

        private async Task Initialize()
        {
            await Task.Run(() => { while (DiscordClient == null) { } });

            DiscordClient.ComponentInteractionCreated += Rules_ComponentInteractionCreated;
        }

        private async Task Rules_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (e.Id == "rules")
            {
                e.Handled = true;
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                DiscordMember discordMember = await e.Interaction.Guild.GetMemberAsync(e.Interaction.User.Id);

                if (discordMember == null)
                    return;

                DiscordRole roleToAssign = await RolesFinder.GetRoleIDForRules(e.Guild);

                if (roleToAssign == null)
                    return;

                await discordMember.GrantRoleAsync(roleToAssign);
            }
        }

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

        [Command("setmusicchannel"), RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
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

        [Command("rules"), RequireUserPermissions(Permissions.Administrator)]
        public async Task PostRulesAsync(CommandContext ctx, DiscordRole roleToAssign, DiscordEmoji reactionEmoji, DiscordChannel rulesChannel, string title, string image_uri, [RemainingText] string rulesText)
        {
            DiscordChannel adminChannel = await ChannelFinder.GetChannelFor("admin", ctx);

            if (adminChannel == null)
            {
                await ctx.Channel.SendMessageAsync("Failed to find admin channel.");
                return;
            }

            if (rulesChannel == null || roleToAssign == null || reactionEmoji == null || string.IsNullOrEmpty(rulesText) || string.IsNullOrEmpty(title))
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithDescription(rulesText)
                .WithAuthor(name: ctx.Client.CurrentUser.Username, url: ctx.Client.CurrentUser.AvatarUrl, iconUrl: ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(DiscordColor.Red)
                .WithImageUrl(image_uri);

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                .WithEmbed(embed)
                .AddComponents(new DiscordButtonComponent(DSharpPlus.ButtonStyle.Success, "rules", "Accept", emoji: new DiscordComponentEmoji(reactionEmoji)));

            bool isSuccess = await DatabaseManager.UpdateRoleForGuildRules(ctx.Guild.Id, roleToAssign.Id);

            if (!isSuccess)
            {
                await adminChannel.SendMessageAsync("Failed to save to database.");
                return;
            }

            await rulesChannel.SendMessageAsync(messageBuilder);
        }
    }
}