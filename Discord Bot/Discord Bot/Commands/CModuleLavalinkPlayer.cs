using Discord_Bot.Services;
using Discord_Bot.Services.DataClasses;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    internal class CModuleLavalinkPlayer : BaseCommandModule
    {
        public LavalinkMusicService LavalinkMusicService;
        public GuildManager GuildManager;

        private enum ConnectionStatus
        {
            OK = 0,
            LavalinkNotFound = 1,
            NodeNotFound = 2,
        }

        private enum MessageSeverity
        {
            Positive,
            Neutral,
            Negative
        }

        private class LavalinkConnectionData
        {
            public LavalinkExtension Lavalink;
            public LavalinkNodeConnection Node;

            public ConnectionStatus Status;

            public LavalinkConnectionData(LavalinkExtension lavalink, LavalinkNodeConnection node, ConnectionStatus status)
            {
                Lavalink = lavalink;
                Node = node;
                Status = status;
            }
        }

        private LavalinkConnectionData GetLavalinkNodeConnection(DiscordClient discordClient)
        {
            LavalinkExtension lavalink = GetLavalink(discordClient);

            if (lavalink == null)
                return new LavalinkConnectionData(lavalink: null, node: null, status: ConnectionStatus.LavalinkNotFound);

            if (!lavalink.ConnectedNodes.Any())
                return new LavalinkConnectionData(lavalink: lavalink, node: null, status: ConnectionStatus.NodeNotFound);

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();

            return new LavalinkConnectionData(lavalink: lavalink, node: node, status: ConnectionStatus.OK);
        }

        private LavalinkExtension GetLavalink(DiscordClient discordClient) => discordClient.GetLavalink();

        private DiscordMessageBuilder CreatePrepareChannelMessage(string prepareContent)
        {
            DiscordEmbedBuilder prepareEmbedBuilder = new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithTitle(prepareContent);
            DiscordMessageBuilder prepareMessageBuilder = new DiscordMessageBuilder().WithEmbed(prepareEmbedBuilder);

            return prepareMessageBuilder;
        }

        [Command("setupmusic"), RequireUserPermissions(DSharpPlus.Permissions.Administrator)]
        public async Task SetupMusicChannel(CommandContext ctx)
        {
            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null) return;

            MusicPlayerData musicPlayerData = LavalinkMusicService.CreateMusicPlayerData(ctx.Guild.Id);

            ctx.Client.ModalSubmitted += MusicPlayerModalSubmitted;
            ctx.Client.ComponentInteractionCreated += MusicPlayerComponentInteractionCreated;

            string prepareContent = "Preparing Channel for Music.";
            DiscordMessageBuilder prepareMessageBuilder = CreatePrepareChannelMessage(prepareContent);
            DiscordMessage prepareMessage = await musicChannel.SendMessageAsync(prepareMessageBuilder);

            DiscordEmbedBuilder searchEmbedBuilder = new DiscordEmbedBuilder().WithColor(DiscordColor.Red).WithTitle("Music Player").WithDescription("Join : Bot will join your voice channel.\nSearch : Enter a search term or link to add songs to playlist.");

            var search = new DiscordMessageBuilder()
                .WithEmbed(searchEmbedBuilder)
                .AddComponents(new DiscordComponent[]
                    {
                        new DiscordButtonComponent(style: ButtonStyle.Success, customId: "music_join", label: "Join", disabled: false),
                        new DiscordButtonComponent (style: ButtonStyle.Primary, customId: "music_search", label: "Search", disabled: false),
                        new DiscordButtonComponent(style: ButtonStyle.Danger, customId: "music_leave", label: "Leave", disabled: false)
                    }
                );

            musicPlayerData.DisplaySearch = await ctx.Channel.SendMessageAsync(search);

            prepareContent += "\n + Added search box to search for tracks.";
            prepareMessageBuilder = CreatePrepareChannelMessage(prepareContent);
            await prepareMessage.ModifyAsync(prepareMessageBuilder);

            musicPlayerData.DisplayError = await musicChannel.SendMessageAsync("Everything working as intended.");

            DiscordMessageBuilder queueBuilder = GetQueueList(musicPlayerData, ctx.Client);

            musicPlayerData.DisplayPlaylist = await musicChannel.SendMessageAsync(queueBuilder);

            await prepareMessage.DeleteAsync();
        }

        private DiscordMessageBuilder GetQueueList(MusicPlayerData musicPlayerData, DiscordClient discordClient)
        {
            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();

            if (musicPlayerData.Playlist.Count == 0)
            {
                embedBuilder.WithColor(DiscordColor.Red)
                            .AddField("Playlist", "There is nothing in the queue to play.");

                messageBuilder.WithEmbed(embedBuilder);

                return messageBuilder;
            }

            string queueContent = "";

            int startIndex = musicPlayerData.CurrentViewPage;
            int endIndex = startIndex + 10 < musicPlayerData.Playlist.Count ? startIndex + 10 : musicPlayerData.Playlist.Count;

            for (int i = startIndex; i < endIndex; i++)
            {
                queueContent += $"{i}) {musicPlayerData.Playlist[i].Track.Title} (Added by {musicPlayerData.Playlist[i].DiscordMemberName})\n";
            }

            DiscordButtonComponent showNext = new DiscordButtonComponent(style: ButtonStyle.Secondary,
                customId: "music_show_next",
                label: "Show Next Page",
                disabled: startIndex == 0,
                emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":arrow_left:")));

            DiscordButtonComponent showPrevious = new DiscordButtonComponent(style: ButtonStyle.Secondary,
                customId: "music_show_previous",
                label: "Show Previous Page",
                disabled: startIndex == 0,
                emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":arrow_left:")));

            DiscordButtonComponent left = new DiscordButtonComponent(style: ButtonStyle.Secondary,
                customId: "music_left",
                label: "Left",
                disabled: startIndex == 0,
                emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":arrow_left:")));

            DiscordButtonComponent right = new DiscordButtonComponent
                (style: ButtonStyle.Secondary,
                customId: "music_right",
                label: "Right",
                disabled: endIndex < musicPlayerData.Playlist.Count,
                emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":arrow_right:")));

            DiscordButtonComponent play = new DiscordButtonComponent
                (style: ButtonStyle.Secondary,
                customId: "music_play",
                label: "Play",
                disabled: musicPlayerData.CurrentTrackIndex < musicPlayerData.Playlist.Count,
                emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":play_pause:")));

            DiscordButtonComponent stop = new DiscordButtonComponent
                (style: ButtonStyle.Secondary,
                customId: "music_stop",
                label: "Stop",
                disabled: true,
                emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":stop_button:")));

            embedBuilder = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .AddField("Playlist", queueContent);

            messageBuilder.WithEmbed(embedBuilder)
                .AddComponents(new DiscordComponent[] { showPrevious, showNext })
                .AddComponents(new DiscordComponent[] { left, right, play, stop });

            return messageBuilder;
        }

        private bool IsMemberInVoiceChannel(DiscordMember discordMember)
        {
            if (discordMember?.VoiceState?.Channel == null)
                return false;

            return true;
        }

        private async Task SendMessageToChannel(DiscordChannel channel, string content, MessageSeverity severity)
        {
            await channel.SendMessageAsync(content);
        }

        public async Task LeaveChannel(DiscordClient discordClient, DiscordMember discordMember, DiscordGuild discordGuild)
        {
            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", discordGuild);
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", discordGuild);

            if (musicChannel == null)
            {
                await SendMessageToChannel(adminChannel, "You have not set music channel.", MessageSeverity.Negative);
                return;
            }

            MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
            {
                await SendMessageToChannel(adminChannel, "Failed to find musicplayer data.", MessageSeverity.Negative);
                return;
            }

            LavalinkConnectionData data = GetLavalinkNodeConnection(discordClient);

            if (data.Status != ConnectionStatus.OK)
            {
                string error = "";

                switch (data.Status)
                {
                    case ConnectionStatus.OK:
                        break;

                    case ConnectionStatus.LavalinkNotFound:
                        error += "Failed to connect to Lavalink service.\n";
                        break;

                    case ConnectionStatus.NodeNotFound:
                        error += "Failed to find a connection node.\n";
                        break;

                    default:
                        break;
                }

                await musicPlayerData.DisplayError.ModifyAsync(error);

                return;
            }

            if (!IsMemberInVoiceChannel(discordMember))
            {
                string error = "You aren't in a voice channel.";

                await musicPlayerData.DisplayError.ModifyAsync(error);

                return;
            }

            LavalinkGuildConnection guildConnection = data.Node.GetGuildConnection(discordGuild);

            if (guildConnection != null)
                if (guildConnection.Channel == discordMember.VoiceState?.Channel)
                    await guildConnection.DisconnectAsync();
        }

        public async Task JoinChannel(DiscordClient discordClient, DiscordMember discordMember, DiscordGuild discordGuild)
        {
            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", discordGuild);
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", discordGuild);

            if (musicChannel == null)
            {
                await SendMessageToChannel(adminChannel, "You have not set music channel.", MessageSeverity.Negative);
                return;
            }

            MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
            {
                await SendMessageToChannel(adminChannel, "Failed to find musicplayer data.", MessageSeverity.Negative);
                return;
            }

            LavalinkConnectionData data = GetLavalinkNodeConnection(discordClient);

            if (data.Status != ConnectionStatus.OK)
            {
                string error = "";

                switch (data.Status)
                {
                    case ConnectionStatus.OK:
                        break;

                    case ConnectionStatus.LavalinkNotFound:
                        error += "Failed to connect to Lavalink service.\n";
                        break;

                    case ConnectionStatus.NodeNotFound:
                        error += "Failed to find a connection node.\n";
                        break;

                    default:
                        break;
                }

                await musicPlayerData.DisplayError.ModifyAsync(error);

                return;
            }

            if (!IsMemberInVoiceChannel(discordMember))
            {
                string error = "You aren't in a voice channel.";

                await musicPlayerData.DisplayError.ModifyAsync(error);

                return;
            }

            LavalinkGuildConnection guildConnection = data.Node.GetGuildConnection(discordGuild);

            if (guildConnection != null)
                if (guildConnection.Channel == discordMember.VoiceState.Channel)
                    return;
                else
                    await guildConnection.DisconnectAsync();

            await data.Node.ConnectAsync(discordMember.VoiceState.Channel);
        }

        private async Task MusicPlayerComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder().WithContent("RESPONE BUILDER");
            //await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, responseBuilder);
            if (e.Id == "music_join")
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                DiscordMember discordMember = await e.Interaction.Guild.GetMemberAsync(e.Interaction.User.Id);

                if (discordMember != null)
                    await JoinChannel(discordClient: sender, discordMember: discordMember, discordGuild: e.Guild);

                return;
            }

            //if(e.Id == "music_search")
            //{
            //    DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder()
            //        .WithCustomId("music_modal_search")
            //        .WithTitle("Enter search term or url..")
            //        .AddComponents(new TextInputComponent(label: "Search : ", customId: "music_search_url"));

            //    await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);
            //}

            //if(e.Id == "music_leave")
            //{
            //    DiscordMember discordMember = e.Guild.Members.FirstOrDefault(c => c.Key == e.User.Id).Value;

            //    if (discordMember != null)
            //        await LeaveChannel(discordClient: sender, discordMember: discordMember, discordGuild: e.Guild);

            //    return;
            //}

            //if(e.Id == "music_previous")
            //{
            //    MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(e.Guild.Id);

            //    if (musicPlayerData.CurrentViewPage == 0)
            //        return;

            //    musicPlayerData.CurrentViewPage -= 1;

            //    DiscordMessageBuilder queueBuilder = GetQueueList(musicPlayerData, sender);

            //    await musicPlayerData.DisplayPlaylist.ModifyAsync(queueBuilder);
            //}

            //if (e.Id == "music_next")
            //{
            //    MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(e.Guild.Id);

            //    if (musicPlayerData.CurrentViewPage > (int) (musicPlayerData.Playlist.Count / 10 - 1))
            //        return;

            //    musicPlayerData.CurrentViewPage += 1;

            //    DiscordMessageBuilder queueBuilder = GetQueueList(musicPlayerData, sender);

            //    await musicPlayerData.DisplayPlaylist.ModifyAsync(queueBuilder);
            //}

            //if(e.Id == "music_left")
            //{
            //}

            //if(e.Id == "music_right")
            //{
            //}

            //if(e.Id == "music_play")
            //{
            //}

            //if(e.Id == "music_stop")
            //{
            //}
        }

        private async Task MusicPlayerModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
        {
            if (e.Interaction.Data.CustomId == "music_modal_search")
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);

                string url = e.Values["music_search_url"];

                DiscordMember discordMember = await Task.Run(() => e.Interaction.Guild.Members.Where(c => c.Key == e.Interaction.User.Id).Select(c => c.Value).First());

                if (discordMember == null)
                    return;

                await Task.Run(() => AddSongsToQueue(discordClient: sender, url: url, discordMemberName: discordMember.DisplayName, guildId: e.Interaction.Guild.Id));
            }

            await Task.Run(() => { });
        }

        private async Task AddSongsToQueue(DiscordClient discordClient, string url, string discordMemberName, ulong guildId)
        {
            LavalinkConnectionData connectionData = GetLavalinkNodeConnection(discordClient);

            var result = await connectionData.Node.Rest.GetTracksAsync(url, LavalinkSearchType.Youtube);

            MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(guildId);

            musicPlayerData.AddTracksToPlaylist(result.Tracks, discordMemberName);

            DiscordMessageBuilder queueBuilder = GetQueueList(musicPlayerData, discordClient);

            await musicPlayerData.DisplayPlaylist.ModifyAsync(queueBuilder);
        }
    }
}