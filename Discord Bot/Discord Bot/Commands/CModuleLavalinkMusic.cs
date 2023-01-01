using Discord_Bot.DataClasses;
using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    //[Group("Music")]
    //[Description("This is all the music commands I have at the moment. Feel free to let me know if you need more.")]
    internal class CModuleLavalinkMusic : BaseCommandModule
    {
        public LavalinkMusicService LavalinkMusicService;
        public DiscordEmbedBuilderHelper EmbedHelper;
        public ChannelFinder ChannelFinder;

        private enum MessageStatus
        {
            LavalinkNotFound,
            NodeNotFound,
            GuildNotFound,
            MusicChannelNotFound,
            UserNotInVoiceChannel,
            UserInSameChannelAsBot,
            BotLeftVoiceChannel,
            BotNotInVoiceChannel,
            NoTracksFound,
            MusicPlayerDataMissing,
            JoinedVoiceChannel,
            PlaylistCleared,
            PlaylistShuffled
        }

        private enum MessageSeverity
        {
            Positive,
            Neutral,
            Negative
        }

        private async Task SendMessageToChannel(DiscordClient discordClient, MessageStatus status, DiscordChannel channelToSendMessage, MessageSeverity messageSeverity)
        {
            if (channelToSendMessage == null || discordClient == null)
                return;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithAuthor(name: discordClient.CurrentUser.Username, url: discordClient.CurrentUser.AvatarUrl, iconUrl: discordClient.CurrentUser.AvatarUrl)
                .WithColor(messageSeverity == MessageSeverity.Positive ? DiscordColor.Green : messageSeverity == MessageSeverity.Negative ? DiscordColor.Red : DiscordColor.White);

            string content = "";

            switch (status)
            {
                case MessageStatus.LavalinkNotFound:
                    content = "I've failed to connect to lavalink service.";
                    break;

                case MessageStatus.NodeNotFound:
                    content = "I've failed to find a suitable node.";
                    break;

                case MessageStatus.GuildNotFound:
                    content = "I've failed to connect.";
                    break;

                case MessageStatus.MusicChannelNotFound:
                    content = "I've failed to find the music channel.";
                    break;

                case MessageStatus.UserNotInVoiceChannel:
                    content = "You have to be in a voice channel for me to join you.";
                    break;

                case MessageStatus.UserInSameChannelAsBot:
                    content = "I'm already there with you.";
                    break;

                case MessageStatus.BotLeftVoiceChannel:
                    content = "I've left the voice channel.";
                    break;

                case MessageStatus.BotNotInVoiceChannel:
                    content = "I'm not in any voice channel.";
                    break;

                case MessageStatus.NoTracksFound:
                    content = "I've failed to find any tracks.";
                    break;

                case MessageStatus.MusicPlayerDataMissing:
                    content = "It seems I haven't fully initialized. Make sure I'm added to a voice channel by the join command.";
                    break;

                case MessageStatus.JoinedVoiceChannel:
                    content = "I've joined the voice channel.";
                    break;

                case MessageStatus.PlaylistCleared:
                    content = "I've cleared the playlist.";
                    break;

                case MessageStatus.PlaylistShuffled:
                    content = "I've shuffled the playlist.";
                    break;

                default:
                    break;
            }

            embed.WithTitle(content);

            await channelToSendMessage.SendMessageAsync(embed);
        }

        private async Task<LavalinkExtension> GetLavalinkAsync(DiscordClient discordClient, DiscordChannel channelToMessage)
        {
            if (discordClient == null || channelToMessage == null) return null;

            LavalinkExtension lavalink = discordClient.GetLavalink();

            if (lavalink == null)
                await SendMessageToChannel(
                    discordClient: discordClient,
                    status: MessageStatus.LavalinkNotFound,
                    channelToSendMessage: channelToMessage,
                    MessageSeverity.Negative);

            return lavalink;
        }

        private async Task<LavalinkNodeConnection> GetLavalinkNodeConnectionAsync(DiscordClient discordClient, DiscordChannel channelToMessage, LavalinkExtension lavalink)
        {
            if (discordClient == null || channelToMessage == null || lavalink == null) return null;

            LavalinkNodeConnection nodeConnection = lavalink.GetIdealNodeConnection();

            if (nodeConnection == null)
                await SendMessageToChannel(
                    discordClient: discordClient,
                    status: MessageStatus.NodeNotFound,
                    channelToSendMessage: channelToMessage,
                    MessageSeverity.Negative);

            return nodeConnection;
        }

        private async Task<bool> IsMemberInVoiceChannel(DiscordMember discordMember, DiscordClient discordClient, DiscordChannel channelToMessage)
        {
            if (discordMember?.VoiceState?.Channel == null)
            {
                await SendMessageToChannel(discordClient, MessageStatus.UserNotInVoiceChannel, channelToMessage, MessageSeverity.Negative);
                return false;
            }

            return true;
        }

        private async Task<(MusicPlayerData, DiscordChannel)> GetMusicPlayerDataAndMusicChannel(DiscordClient discordClient, DiscordGuild discordGuild, DiscordChannel altChannelToSendMessage, DiscordMember discordMember)
        {
            DiscordChannel musicChannel = await ChannelFinder.GetChannelFor("music", discordGuild);

            if (musicChannel == null)
            {
                await SendMessageToChannel(discordClient, MessageStatus.MusicChannelNotFound, altChannelToSendMessage, MessageSeverity.Negative);
                return (null, null);
            }

            bool isMemberInVoiceChannel = await IsMemberInVoiceChannel(discordMember, discordClient, musicChannel);
            if (!isMemberInVoiceChannel) return (null, null);

            MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData?.LavalinkGuildConnection?.Channel == null)
            {
                await SendMessageToChannel(
                         discordClient: discordClient,
                         status: MessageStatus.BotNotInVoiceChannel,
                         channelToSendMessage: musicChannel,
                         messageSeverity: MessageSeverity.Negative);

                return (null, null);
            }

            return (musicPlayerData, musicChannel);
        }

        [Command("join")]
        public async Task JoinChannelAsync(CommandContext ctx)
        {
            DiscordChannel musicChannel = await ChannelFinder.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await SendMessageToChannel(ctx.Client, MessageStatus.MusicChannelNotFound, ctx.Channel, MessageSeverity.Negative);
                return;
            }

            LavalinkExtension lavalink = await GetLavalinkAsync(ctx.Client, musicChannel);
            if (lavalink == null) return;

            LavalinkNodeConnection nodeConnection = await GetLavalinkNodeConnectionAsync(ctx.Client, musicChannel, lavalink);
            if (nodeConnection == null) return;

            bool isMemberInVoiceChannel = await IsMemberInVoiceChannel(ctx.Member, ctx.Client, musicChannel);
            if (!isMemberInVoiceChannel) return;

            LavalinkGuildConnection guildConnection = nodeConnection.GetGuildConnection(ctx.Guild);

            if (guildConnection != null)
            {
                if (guildConnection.Channel == ctx.Member.VoiceState.Channel)
                {
                    await SendMessageToChannel(
                        discordClient: ctx.Client,
                        status: MessageStatus.UserInSameChannelAsBot,
                        channelToSendMessage: musicChannel,
                        messageSeverity: MessageSeverity.Neutral);

                    return;
                }
                else if (guildConnection.Channel != null)
                    await guildConnection.DisconnectAsync(shouldDestroy: true);
            }

            guildConnection = await nodeConnection.ConnectAsync(ctx.Member.VoiceState.Channel);

            MusicPlayerData musicPlayerData = LavalinkMusicService.CreateMusicPlayerData(ctx.Guild.Id);
            musicPlayerData.LavalinkGuildConnection = guildConnection;

            guildConnection.PlaybackStarted += GuildConnection_PlaybackStarted;
            guildConnection.PlaybackFinished += GuildConnection_PlaybackFinished;
            guildConnection.PlayerUpdated += GuildConnection_PlayerUpdated;

            await SendMessageToChannel(
                        discordClient: ctx.Client,
                        status: MessageStatus.JoinedVoiceChannel,
                        channelToSendMessage: musicChannel,
                        messageSeverity: MessageSeverity.Positive);
        }

        private async Task GuildConnection_PlayerUpdated(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.PlayerUpdateEventArgs e)
        {
            DiscordChannel musicChannel = await ChannelFinder.GetChannelFor("music", sender.Guild);
            //e.Handled = true;
            //await musicChannel.SendMessageAsync("PLAYBACK UPDATED");
        }

        private async Task GuildConnection_PlaybackStarted(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackStartEventArgs e)
        {
            DiscordChannel musicChannel = await ChannelFinder.GetChannelFor("music", sender.Guild);
            MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(sender.Guild.Id);

            TrackData trackData = musicPlayerData.GetCurrentTrack();
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Now Playing")
                .WithDescription($"{trackData.Track.Title} (Added By {trackData.DiscordMemberName})")
                .WithColor(DiscordColor.Rose)
                .WithAuthor(name: sender.Guild.CurrentMember.Username, url: sender.Guild.CurrentMember.AvatarUrl, iconUrl: sender.Guild.CurrentMember.AvatarUrl);

            await musicChannel.SendMessageAsync(embed);
        }

        private async Task GuildConnection_PlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(sender.Guild.Id);
            if (musicPlayerData == null)
                return;

            LavalinkTrack lavalinkTrack = musicPlayerData.GetNextTrack().Track;
            if (lavalinkTrack == null)
            {
                musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Stop;
                return;
            }

            await musicPlayerData.LavalinkGuildConnection.PlayAsync(lavalinkTrack);
        }

        [Command("leave")]
        public async Task LeaveChannelAsync(CommandContext ctx)
        {
            DiscordChannel musicChannel = await ChannelFinder.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await SendMessageToChannel(ctx.Client, MessageStatus.MusicChannelNotFound, ctx.Channel, MessageSeverity.Negative);
                return;
            }

            bool isMemberInVoiceChannel = await IsMemberInVoiceChannel(ctx.Member, ctx.Client, musicChannel);
            if (!isMemberInVoiceChannel) return;

            MusicPlayerData musicPlayerData = LavalinkMusicService.GetMusicPlayerData(ctx.Guild.Id);
            if (musicPlayerData == null) return;

            if (musicPlayerData.LavalinkGuildConnection != null)
            {
                musicPlayerData.LavalinkGuildConnection.PlaybackFinished -= GuildConnection_PlaybackFinished;
                musicPlayerData.LavalinkGuildConnection.PlayerUpdated -= GuildConnection_PlayerUpdated;
                musicPlayerData.LavalinkGuildConnection.PlaybackStarted -= GuildConnection_PlaybackStarted;

                await musicPlayerData.LavalinkGuildConnection.DisconnectAsync(shouldDestroy: true);
                await SendMessageToChannel(
                         discordClient: ctx.Client,
                         status: MessageStatus.BotLeftVoiceChannel,
                         channelToSendMessage: musicChannel,
                         messageSeverity: MessageSeverity.Positive);
            }
            else
            {
                await SendMessageToChannel(
                        discordClient: ctx.Client,
                        status: MessageStatus.BotNotInVoiceChannel,
                        channelToSendMessage: musicChannel,
                        messageSeverity: MessageSeverity.Neutral);
            }
        }

        [Command("queue")]
        public async Task QueueSongAsync(CommandContext ctx, [RemainingText] Uri search)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            LavalinkLoadResult loadResult = await musicPlayerData.LavalinkGuildConnection.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await SendMessageToChannel(
                        discordClient: ctx.Client,
                        status: MessageStatus.NoTracksFound,
                        channelToSendMessage: musicChannel,
                        messageSeverity: MessageSeverity.Negative);
                return;
            }

            musicPlayerData.AddATrackToPlaylist(loadResult.Tracks.First(), ctx.Member.DisplayName);

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Stop)
            {
                LavalinkTrack track = musicPlayerData.GetNextTrack().Track;
                await musicPlayerData.LavalinkGuildConnection.PlayAsync(track);

                musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Play;
            }
        }

        [Command("queue")]
        public async Task QueueSongAsync(CommandContext ctx, [RemainingText] string search)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            LavalinkLoadResult loadResult = await musicPlayerData.LavalinkGuildConnection.GetTracksAsync(search, LavalinkSearchType.Youtube);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await SendMessageToChannel(
                        discordClient: ctx.Client,
                        status: MessageStatus.NoTracksFound,
                        channelToSendMessage: musicChannel,
                        messageSeverity: MessageSeverity.Negative);
                return;
            }

            musicPlayerData.AddATrackToPlaylist(loadResult.Tracks.First(), ctx.Member.DisplayName);

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Stop)
            {
                LavalinkTrack track = musicPlayerData.GetNextTrack().Track;
                await musicPlayerData.LavalinkGuildConnection.PlayAsync(track);
                await musicPlayerData.LavalinkGuildConnection.ResumeAsync();

                musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Play;
            }
        }

        [Command("queuepl")]
        public async Task QueuePlaylistAsync(CommandContext ctx, [RemainingText] Uri search)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            LavalinkLoadResult loadResult = await musicPlayerData.LavalinkGuildConnection.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await SendMessageToChannel(
                        discordClient: ctx.Client,
                        status: MessageStatus.NoTracksFound,
                        channelToSendMessage: musicChannel,
                        messageSeverity: MessageSeverity.Negative);
                return;
            }

            musicPlayerData.AddTracksToPlaylist(loadResult.Tracks, ctx.Member.DisplayName);

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Stop)
            {
                LavalinkTrack track = musicPlayerData.GetNextTrack().Track;
                await musicPlayerData.LavalinkGuildConnection.PlayAsync(track);

                musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Play;
            }
        }

        [Command("pause")]
        public async Task PauseSongAsync(CommandContext ctx)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Play)
            {
                await musicPlayerData.LavalinkGuildConnection.PauseAsync();
            }
        }

        [Command("resume")]
        public async Task ResumeSongAsync(CommandContext ctx)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Pause)
            {
                await musicPlayerData.LavalinkGuildConnection.ResumeAsync();
            }
        }

        [Command("stop"), Aliases("skip")]
        public async Task StopSongAsync(CommandContext ctx)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            LavalinkTrack lavalinkTrack = musicPlayerData.GetNextTrack().Track;
            await musicPlayerData.LavalinkGuildConnection.PlayAsync(lavalinkTrack);
        }

        [Command("clear")]
        public async Task ClearPlaylistAsync(CommandContext ctx)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            musicPlayerData.ClearPlaylist();

            await SendMessageToChannel(
                         discordClient: ctx.Client,
                         status: MessageStatus.PlaylistCleared,
                         channelToSendMessage: musicChannel,
                         messageSeverity: MessageSeverity.Positive);
        }

        [Command("shuffle")]
        public async Task ShufflePlaylistAsync(CommandContext ctx)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            await musicPlayerData.ShufflePlaylist();

            await SendMessageToChannel(
                         discordClient: ctx.Client,
                         status: MessageStatus.PlaylistShuffled,
                         channelToSendMessage: musicChannel,
                         messageSeverity: MessageSeverity.Positive);
        }

        [Command("showqueue")]
        public async Task ShowQueueAsync(CommandContext ctx)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            InteractivityExtension interactivity = ctx.Client.GetInteractivity();

            string content = "";

            for (int i = 0; i < musicPlayerData.Playlist.Count; i++)
            {
                content += $"{i + 1}) {musicPlayerData.Playlist[i].Track.Title} (Added by {musicPlayerData.Playlist[i].DiscordMemberName})\n";
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Rose)
                .WithAuthor(name: ctx.Client.CurrentUser.Username, url: ctx.Client.CurrentUser.AvatarUrl, iconUrl: ctx.Client.CurrentUser.AvatarUrl);

            var pages = interactivity.GeneratePagesInEmbed(content, splittype: DSharpPlus.Interactivity.Enums.SplitType.Character, embedbase: embed);

            await musicChannel.SendPaginatedMessageAsync(ctx.Member, pages);
        }

        [Command("np")]
        public async Task ShowNowPlayingAsync(CommandContext ctx)
        {
            var result = await GetMusicPlayerDataAndMusicChannel(
                discordClient: ctx.Client,
                discordGuild: ctx.Guild,
                altChannelToSendMessage: ctx.Channel,
                discordMember: ctx.Member);

            MusicPlayerData musicPlayerData = result.Item1;
            DiscordChannel musicChannel = result.Item2;

            if (musicPlayerData == null || musicChannel == null) return;

            TrackData trackData = musicPlayerData.GetCurrentTrack();
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Now Playing")
                .WithDescription($"{trackData.Track.Title} (Added By {trackData.DiscordMemberName})")
                .WithColor(DiscordColor.Rose)
            .WithAuthor(name: ctx.Client.CurrentUser.Username, url: ctx.Client.CurrentUser.AvatarUrl, iconUrl: ctx.Client.CurrentUser.AvatarUrl);

            await musicChannel.SendMessageAsync(embed);
        }
    }
}