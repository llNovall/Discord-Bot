using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    [RequireUserPermissions(Permissions.Administrator)]
    internal class CModuleMessageBuilder : BaseCommandModule
    {
        public GuildManager GuildManager;
        public Helper Helper;
        public DiscordClient DiscordClient;

        private Dictionary<ulong, DiscordMessageBuilder> _messageBuilderDict = new();
        private Dictionary<string, DiscordComponent> _componentDict = new();

        public CModuleMessageBuilder()
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
            var rolesDict = GuildManager.GetRolesFor(e.Guild);

            if (rolesDict.ContainsKey(e.Id))
            {
                e.Handled = true;
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                var discordMember = await e.Interaction.Guild.GetMemberAsync(e.Interaction.User.Id);

                if (discordMember == null)
                    return;

                var roleToAssign = GuildManager.GetRoleFor(e.Guild, e.Id);

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
                        await Helper.SendMessageToChannelAsync(DiscordClient, adminChannel, Helper.MessageSeverity.Negative, "Failed to assign role", $"Attempted to give role {roleToAssign.Name} to {discordMember.Mention} but failed.\nReason : {ex.Message}");
                }
            }
        }

        private async Task<DiscordMessageBuilder> GetMessageBuilderAsync(DiscordClient client, DiscordGuild discordGuild, DiscordChannel channelToMessage)
        {
            if (_messageBuilderDict.ContainsKey(discordGuild.Id)) return _messageBuilderDict[discordGuild.Id];

            await Helper.SendMessageToChannelAsync(client, channelToMessage, Helper.MessageSeverity.Negative, "Run the command **mb** before using this command.");
            return null;
        }

        [Command("mb")]
        public async Task CreateEmptyMessageBuilderAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, "Failed to find admin channel.");
                return;
            }

            if (_messageBuilderDict.ContainsKey(ctx.Guild.Id))
                _messageBuilderDict[ctx.Guild.Id] = new DiscordMessageBuilder();
            else
                _messageBuilderDict.Add(ctx.Guild.Id, new DiscordMessageBuilder());

            await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Positive, "Successfully created empty message builder.");
        }

        [Command("mb_addrole")]
        public async Task AddRoleAsync(CommandContext ctx, string roleUsage, DiscordRole discordRole)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            if (string.IsNullOrEmpty(roleUsage))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, "Role Usage is empty.");
                return;
            }

            if (discordRole == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, "Role is empty.");
                return;
            }

            await (await GuildManager.UpdateRoleFor(ctx.Guild, discordRole, roleUsage) ?
                Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Positive, "Successfully added Role", $"Successfully added {discordRole.Mention} for {roleUsage}.")
              : Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, "Failed to add Role.", $"Failed to set {discordRole.Mention} for {roleUsage}."));
        }

        [Command("mb_roles")]
        public async Task GetRolesAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            Dictionary<string, DiscordRole> rolesDict = GuildManager.GetRolesFor(ctx.Guild);
            string content = rolesDict.Aggregate("", (current, role) => current + $"{role.Key} - {role.Value.Mention}\n");

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Roles added with usage")
                .WithDescription(string.IsNullOrEmpty(content) ? "No Roles added with usage." : content)
                .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl, ctx.Client.CurrentUser.AvatarUrl)
                .WithColor(DiscordColor.MidnightBlue);

            await adminChannel.SendMessageAsync(embed);
        }

        [Command("mb_btncreate")]
        public async Task AddButtonForRoleToMessageBuilderAsync(CommandContext ctx, string roleUsage, int style, string label = "", DiscordEmoji discordEmoji = null)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            if (string.IsNullOrWhiteSpace(roleUsage))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Role usage is missing.");
                return;
            }

            if (style is < 1 or > 4)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Style is out of boundary. Keep it within 1-4");
                return;
            }

            DiscordRole role = GuildManager.GetRoleFor(ctx.Guild, roleUsage);

            if (role == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find role", $"Failed to find role for {roleUsage}.");
                return;
            }

            DiscordButtonComponent btnRole = new DiscordButtonComponent((ButtonStyle)style, roleUsage, label, false, new DiscordComponentEmoji(discordEmoji));

            if (_componentDict.ContainsKey(roleUsage))
                _componentDict[roleUsage] = btnRole;
            else
                _componentDict.Add(roleUsage, btnRole);

            await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Positive, "Successfully created button", $"Successfully created button for {roleUsage}.");
            return;
        }

        [Command("mb_preview")]
        public async Task PreviewMessageBuilderAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            DiscordMessageBuilder messageBuilder = await GetMessageBuilderAsync(ctx.Client, ctx.Guild, adminChannel);

            if (messageBuilder == null)
                return;

            if (string.IsNullOrWhiteSpace(messageBuilder.Content) && messageBuilder.Embeds.Count == 0)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Add message or embed before previewing.");
                return;
            }

            var message = await messageBuilder.SendAsync(adminChannel);

            await Task.Delay(TimeSpan.FromSeconds(5));
            await message.DeleteAsync();
        }

        [Command("mb_btnadd")]
        public async Task AddButtonsToMessageAsync(CommandContext ctx, [RemainingText] string ids)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            DiscordMessageBuilder messageBuilder = await GetMessageBuilderAsync(ctx.Client, ctx.Guild, adminChannel);

            if (messageBuilder == null)
                return;

            if (string.IsNullOrWhiteSpace(ids))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Role ids are missing.");
                return;
            }

            string[] buttonIds = ids.ToLower().Trim().Split(',', StringSplitOptions.TrimEntries);

            if (buttonIds.Length is > 5 or < 0)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Keep number of role ids between 1 - 5.");
                return;
            }

            if (string.IsNullOrWhiteSpace(messageBuilder.Content) && messageBuilder.Embeds.Count == 0)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Add message or embed before adding buttons");
                return;
            }

            List<DiscordComponent> discordButtonComponents = new();

            foreach (var button in buttonIds)
            {
                if (_componentDict.ContainsKey(button))
                    discordButtonComponents.Add(_componentDict[button]);
                else
                {
                    await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find button", $"Failed to find button for {button}");
                    return;
                }
            }

            if (messageBuilder.Components.SelectMany(rowComponent => rowComponent.Components).Any(component => discordButtonComponents.Any(c => c.CustomId == component.CustomId)))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to add button", $"Adding same button is not allowed.");
                return;
            }
            messageBuilder.AddComponents(discordButtonComponents);

            try
            {
                var message = await messageBuilder.SendAsync(adminChannel);

                await Task.Delay(TimeSpan.FromSeconds(5));
                await message.DeleteAsync();
            }
            catch (BadRequestException e)
            {
                DiscordClient.Logger.Log(LogLevel.Error, $"{e.Message} : {e.Errors} : {e.Code} : {e.JsonMessage}");
            }
        }

        [Command("mb_message")]
        public async Task AddMessageToMessageBuilderAsync(CommandContext ctx, [RemainingText] string message)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            DiscordMessageBuilder messageBuilder = await GetMessageBuilderAsync(ctx.Client, ctx.Guild, adminChannel);

            if (messageBuilder == null)
                return;

            if (string.IsNullOrWhiteSpace(message))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Message is empty.");
                return;
            }

            messageBuilder.WithContent(message);

            var sendMessage = await messageBuilder.SendAsync(adminChannel);

            await Task.Delay(TimeSpan.FromSeconds(3));

            await sendMessage.DeleteAsync();
        }

        [Command("mb_embed")]
        public async Task SendMessageBuilderToChannelAsync(CommandContext ctx, Uri image, string title, [RemainingText] string description)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            DiscordMessageBuilder messageBuilder = await GetMessageBuilderAsync(ctx.Client, ctx.Guild, adminChannel);

            if (messageBuilder == null)
                return;

            if (string.IsNullOrWhiteSpace(description))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Message is empty.");
                return;
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl, ctx.Client.CurrentUser.AvatarUrl);

            if (!string.IsNullOrWhiteSpace(title))
                embedBuilder.WithTitle(title);

            if (!string.IsNullOrWhiteSpace(description))
                embedBuilder.WithDescription(description);

            embedBuilder.WithColor(DiscordColor.Rose);

            if (image != null)
                embedBuilder.WithImageUrl(image);

            messageBuilder.AddEmbed(embedBuilder);

            var sendMessage = await messageBuilder.SendAsync(adminChannel);

            await Task.Delay(TimeSpan.FromSeconds(3));

            await sendMessage.DeleteAsync();
        }

        [Command("mb_embed")]
        public async Task SendMessageBuilderToChannelAsync(CommandContext ctx, string title, [RemainingText] string description)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            DiscordMessageBuilder messageBuilder = await GetMessageBuilderAsync(ctx.Client, ctx.Guild, adminChannel);

            if (messageBuilder == null)
                return;

            if (string.IsNullOrWhiteSpace(description))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Message is empty.");
                return;
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithAuthor(ctx.Client.CurrentUser.Username, ctx.Client.CurrentUser.AvatarUrl, ctx.Client.CurrentUser.AvatarUrl);

            if (!string.IsNullOrWhiteSpace(title))
                embedBuilder.WithTitle(title);

            if (!string.IsNullOrWhiteSpace(description))
                embedBuilder.WithDescription(description);

            embedBuilder.WithColor(DiscordColor.Rose);

            messageBuilder.AddEmbed(embedBuilder);

            var sendMessage = await messageBuilder.SendAsync(adminChannel);

            await Task.Delay(TimeSpan.FromSeconds(3));

            await sendMessage.DeleteAsync();
        }

        [Command("mb_send")]
        public async Task SendMessageBuilderToChannelAsync(CommandContext ctx, DiscordChannel channelToSend)
        {
            DiscordChannel adminChannel = await Helper.GetAdminChannel(ctx);
            if (adminChannel == null)
                return;

            DiscordMessageBuilder messageBuilder = await GetMessageBuilderAsync(ctx.Client, ctx.Guild, adminChannel);

            if (messageBuilder == null)
                return;

            await messageBuilder.SendAsync(channelToSend);
        }
    }
}