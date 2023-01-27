using Discord_Bot.Services.DataClasses;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    public class LavalinkMusicService
    {
        private SpotifyService _spotifyService;
        private DiscordClient _discordClient;
        private GuildManager _guildManager;

        private enum SearchType
        {
            None,
            Search,
            YoutubePlaylist,
            SpotifyTrack,
            SpotifyPlaylist
        }

        public enum Status
        {
            None,
            LavalinkNotFound,
            NodeNotFound,
            UserNotInVoiceChannel,
            UserNotInSameChannelAsBot,
            UserInSameChannelAsBot,
            BotLeftVoiceChannel,
            BotNotInVoiceChannel,
            NoTracksFound,
            JoinedVoiceChannel,
            PlaylistCleared,
            PlaylistShuffled,
            QueueSuccessful,
            PlayerPaused,
            PlayerResumed,
            PlayerStopped
        }

        private Dictionary<ulong, MusicPlayerData> _musicPlayers = new();

        public LavalinkMusicService(SpotifyService spotifyService, DiscordClient discordClient, GuildManager guildManager)
        {
            _spotifyService = spotifyService;
            _discordClient = discordClient;
            _guildManager = guildManager;
        }

        public MusicPlayerData CreateMusicPlayerData(ulong guildId)
        {
            if (!_musicPlayers.ContainsKey(guildId))
                _musicPlayers.Add(guildId, new MusicPlayerData());
            else
                _musicPlayers[guildId] = new MusicPlayerData();

            return _musicPlayers[guildId];
        }

        public MusicPlayerData GetMusicPlayerData(ulong guildId)
        {
            if (!_musicPlayers.ContainsKey(guildId))
                return null;

            return _musicPlayers[guildId];
        }

        private bool IsMemberInVoiceChannel(DiscordMember discordMember)
        => discordMember?.VoiceState?.Channel != null;

        private SearchType GetSearchType(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return SearchType.None;

            if (new Regex(@"open\.spotify\.com/playlist/(?<id>[\da-zA-Z]*)").IsMatch(search))
                return SearchType.SpotifyPlaylist;

            if (new Regex(@"open\.spotify\.com\/track\/(?<id>[\da-zA-Z]*)").IsMatch(search))
                return SearchType.SpotifyTrack;

            if (new Regex(@"youtube\.[a-z]{2,3}\/playlist").IsMatch(search))
                return SearchType.YoutubePlaylist;

            return SearchType.Search;
        }

        public async Task<Status> JoinChannelAsync(DiscordClient discordClient, DiscordGuild discordGuild, DiscordMember discordMember)
        {
            LavalinkExtension lavalink = discordClient?.GetLavalink();
            if (lavalink == null)
                return Status.LavalinkNotFound;

            LavalinkNodeConnection nodeConnection = lavalink.GetIdealNodeConnection();
            if (nodeConnection == null)
                return Status.NodeNotFound;

            if (!IsMemberInVoiceChannel(discordMember))
                return Status.UserNotInVoiceChannel;

            LavalinkGuildConnection guildConnection = nodeConnection.GetGuildConnection(discordGuild);

            if (guildConnection?.Channel == discordMember.VoiceState.Channel)
                return Status.UserInSameChannelAsBot;

            if (guildConnection?.Channel != null)
                await guildConnection.DisconnectAsync(shouldDestroy: true);

            guildConnection = await nodeConnection.ConnectAsync(discordMember.VoiceState.Channel);

            MusicPlayerData musicPlayerData = CreateMusicPlayerData(discordGuild.Id);
            musicPlayerData.LavalinkGuildConnection = guildConnection;

            guildConnection.PlaybackStarted += GuildConnection_PlaybackStarted;
            guildConnection.PlaybackFinished += GuildConnection_PlaybackFinished;

            return Status.JoinedVoiceChannel;
        }

        public async Task<Status> LeaveChannelAsync(DiscordGuild discordGuild, DiscordMember discordMember)
        {
            if (!IsMemberInVoiceChannel(discordMember))
                return Status.UserNotInVoiceChannel;

            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData?.LavalinkGuildConnection == null) return Status.BotNotInVoiceChannel;

            musicPlayerData.LavalinkGuildConnection.PlaybackFinished -= GuildConnection_PlaybackFinished;
            musicPlayerData.LavalinkGuildConnection.PlaybackStarted -= GuildConnection_PlaybackStarted;

            await musicPlayerData.LavalinkGuildConnection.DisconnectAsync(shouldDestroy: true);
            return Status.BotLeftVoiceChannel;
        }

        public async Task<Status> QueueSongAsync(DiscordGuild discordGuild, DiscordMember discordMember, string search)
        {
            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData?.LavalinkGuildConnection?.Channel == null)
                return Status.BotNotInVoiceChannel;

            if (!IsMemberInVoiceChannel(discordMember))
                return Status.UserNotInVoiceChannel;

            if (discordMember.VoiceState.Channel != musicPlayerData.LavalinkGuildConnection.Channel)
                return Status.UserNotInSameChannelAsBot;

            SearchType searchType = GetSearchType(search);

            switch (searchType)
            {
                case SearchType.None:
                    return Status.NoTracksFound;

                case SearchType.Search:
                    if (await AddSongToQueue(search, musicPlayerData, discordMember))
                        return Status.QueueSuccessful;
                    break;

                case SearchType.YoutubePlaylist:
                    if (await AddYoutubePlaylist(search, musicPlayerData, discordMember))
                        return Status.QueueSuccessful;
                    break;

                case SearchType.SpotifyTrack:
                    string trackName = await _spotifyService.GetTrackName(new Uri(search));
                    if (string.IsNullOrWhiteSpace(trackName))
                        return Status.NoTracksFound;

                    if (await AddSongToQueue(search, musicPlayerData, discordMember))
                        return Status.QueueSuccessful;
                    break;

                case SearchType.SpotifyPlaylist:

                    var trackNames = await _spotifyService.GetTrackNames(new Uri(search));

                    if (trackNames.Count == 0)
                        return Status.NoTracksFound;

                    if (await AddSpotifyPlaylist(trackNames, musicPlayerData, discordMember))
                        return Status.QueueSuccessful;

                    break;
            }

            return Status.NoTracksFound;
        }

        private async Task<bool> AddSongToQueue(string search, MusicPlayerData musicPlayerData, DiscordMember discordMember)
        {
            LavalinkLoadResult loadResult = await musicPlayerData.LavalinkGuildConnection.GetTracksAsync(search);

            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                return false;

            musicPlayerData.AddATrackToPlaylist(loadResult.Tracks.First(), discordMember.DisplayName);

            if (musicPlayerData.CurrentMusicPlayerState != MusicPlayerData.MusicPlayerState.Stop) return true;

            LavalinkTrack track = musicPlayerData.GetNextTrack().Track;

            await musicPlayerData.LavalinkGuildConnection.PlayAsync(track);

            musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Play;

            return true;
        }

        private async Task<bool> AddYoutubePlaylist(string search, MusicPlayerData musicPlayerData, DiscordMember discordMember)
        {
            LavalinkLoadResult loadResult = await musicPlayerData.LavalinkGuildConnection.GetTracksAsync(search);

            if (loadResult.LoadResultType is LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                return false;

            musicPlayerData.AddTracksToPlaylist(loadResult.Tracks, discordMember.DisplayName);

            if (musicPlayerData.CurrentMusicPlayerState != MusicPlayerData.MusicPlayerState.Stop)
                return true;

            LavalinkTrack track = musicPlayerData.GetNextTrack().Track;
            await musicPlayerData.LavalinkGuildConnection.PlayAsync(track);

            musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Play;

            return true;
        }

        private async Task<bool> AddSpotifyPlaylist(IEnumerable<string> trackNames, MusicPlayerData musicPlayerData, DiscordMember discordMember)
        {
            bool isSuccess = false;
            foreach (string trackName in trackNames)
            {
                LavalinkLoadResult loadResult = await musicPlayerData.LavalinkGuildConnection.GetTracksAsync(trackName, LavalinkSearchType.Youtube);
                if (loadResult.LoadResultType is not LavalinkLoadResultType.LoadFailed or LavalinkLoadResultType.NoMatches)
                {
                    isSuccess = true;
                    LavalinkTrack foundTrack = loadResult.Tracks.FirstOrDefault();
                    if (foundTrack == null)
                        continue;

                    musicPlayerData.AddATrackToPlaylist(foundTrack, discordMember.DisplayName);

                    if (musicPlayerData.CurrentMusicPlayerState is MusicPlayerData.MusicPlayerState.Play or MusicPlayerData.MusicPlayerState.Pause) continue;
                    LavalinkTrack track = musicPlayerData.GetNextTrack().Track;
                    await musicPlayerData.LavalinkGuildConnection.PlayAsync(track);

                    musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Play;
                }
            }

            return isSuccess;
        }

        public async Task<Status> PauseSongAsync(DiscordGuild discordGuild)
        {
            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
                return Status.BotNotInVoiceChannel;

            if (musicPlayerData.CurrentMusicPlayerState is MusicPlayerData.MusicPlayerState.Pause or MusicPlayerData.MusicPlayerState.Stop)
                return Status.None;

            await musicPlayerData.LavalinkGuildConnection.PauseAsync();
            return Status.PlayerPaused;
        }

        public async Task<Status> ResumeSongAsync(DiscordGuild discordGuild)
        {
            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
                return Status.BotNotInVoiceChannel;

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Play) return Status.None;

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Pause)
                await musicPlayerData.LavalinkGuildConnection.ResumeAsync();

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Stop)
            {
                var trackData = musicPlayerData.GetNextTrack();
                if (trackData != null)
                    await musicPlayerData.LavalinkGuildConnection.PlayAsync(trackData.Track);
            }

            return Status.PlayerResumed;
        }

        public async Task<Status> StopSongAsync(DiscordGuild discordGuild)
        {
            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
                return Status.BotNotInVoiceChannel;

            if (musicPlayerData.CurrentMusicPlayerState == MusicPlayerData.MusicPlayerState.Stop) return Status.None;

            await musicPlayerData.LavalinkGuildConnection.StopAsync();
            return Status.PlayerStopped;
        }

        public Status ClearPlaylistAsync(DiscordGuild discordGuild)
        {
            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
                return Status.BotNotInVoiceChannel;

            musicPlayerData.ClearPlaylist();

            return Status.PlaylistCleared;
        }

        public async Task<Status> ShufflePlaylistAsync(DiscordGuild discordGuild)
        {
            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
                return Status.BotNotInVoiceChannel;

            await musicPlayerData.ShufflePlaylist();

            return Status.PlaylistShuffled;
        }

        public async Task<Status> ShowQueueAsync(DiscordGuild discordGuild, DiscordChannel musicChannel, DiscordMember discordMember)
        {
            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
                return Status.BotNotInVoiceChannel;

            InteractivityExtension interactivity = _discordClient.GetInteractivity();

            string content = "";

            for (int i = 0; i < musicPlayerData.Playlist.Count; i++)
            {
                content += $"{i + 1}) {musicPlayerData.Playlist[i].Track.Title} (Added by {musicPlayerData.Playlist[i].DiscordMemberName})\n";
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Rose)
                .WithAuthor(name: _discordClient.CurrentUser.Username, url: _discordClient.CurrentUser.AvatarUrl, iconUrl: _discordClient.CurrentUser.AvatarUrl);

            var pages = interactivity.GeneratePagesInEmbed(string.IsNullOrEmpty(content) ? "Playlist is empty.." : content, splittype: DSharpPlus.Interactivity.Enums.SplitType.Character, embedbase: embed);

            await musicChannel.SendPaginatedMessageAsync(discordMember, pages);

            return Status.None;
        }

        public async Task<Status> ShowNowPlayingAsync(DiscordGuild discordGuild, DiscordChannel musicChannel)
        {
            MusicPlayerData musicPlayerData = GetMusicPlayerData(discordGuild.Id);

            if (musicPlayerData == null)
                return Status.BotNotInVoiceChannel;

            TrackData trackData = musicPlayerData.GetCurrentTrack();
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithTitle("Now Playing")
                .WithDescription($"{trackData.Track.Title} (Added By {trackData.DiscordMemberName})")
                .WithColor(DiscordColor.Rose)
                .WithAuthor(name: _discordClient.CurrentUser.Username, url: _discordClient.CurrentUser.AvatarUrl, iconUrl: _discordClient.CurrentUser.AvatarUrl);

            await musicChannel.SendMessageAsync(embed);

            return Status.None;
        }

        private async Task GuildConnection_PlaybackStarted(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackStartEventArgs e)
        {
            DiscordChannel musicChannel = _guildManager.GetChannelFor("music", sender.Guild);
            MusicPlayerData musicPlayerData = GetMusicPlayerData(sender.Guild.Id);

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
            MusicPlayerData musicPlayerData = GetMusicPlayerData(sender.Guild.Id);
            if (musicPlayerData == null)
                return;

            TrackData trackData = musicPlayerData.GetNextTrack();

            if (trackData == null)
            {
                musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Stop;
                return;
            }
            LavalinkTrack lavalinkTrack = trackData.Track;

            await musicPlayerData.LavalinkGuildConnection.PlayAsync(lavalinkTrack);
            musicPlayerData.CurrentMusicPlayerState = MusicPlayerData.MusicPlayerState.Play;
        }
    }
}