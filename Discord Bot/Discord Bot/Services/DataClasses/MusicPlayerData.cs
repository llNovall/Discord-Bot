using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.Services.DataClasses
{
    public class MusicPlayerData
    {
        public enum MusicPlayerState
        {
            Play,
            Pause,
            Stop,
        }

        private int _currentTrackIndex;
        private List<TrackData> _playlist;
        private DiscordMessage _displayPlaylist;
        private DiscordMessage _displaySearch;
        private DiscordMessage _displayError;
        private LavalinkGuildConnection _guildConnection;

        private int _currentViewPage;
        private MusicPlayerState _currentMusicPlayerState;
        public int CurrentTrackIndex { get => _currentTrackIndex; set => _currentTrackIndex = value; }
        public DiscordMessage DisplayPlaylist { get => _displayPlaylist; set => _displayPlaylist = value; }
        public DiscordMessage DisplaySearch { get => _displaySearch; set => _displaySearch = value; }
        public DiscordMessage DisplayError { get => _displayError; set => _displayError = value; }
        public List<TrackData> Playlist { get => _playlist; }
        public int CurrentViewPage { get => _currentViewPage; set => _currentViewPage = value; }
        public MusicPlayerState CurrentMusicPlayerState { get => _currentMusicPlayerState; set => _currentMusicPlayerState = value; }
        public LavalinkGuildConnection LavalinkGuildConnection { get => _guildConnection; set => _guildConnection = value; }

        public MusicPlayerData()
        {
            _playlist = new List<TrackData>();
            _currentTrackIndex = -1;
            _currentMusicPlayerState = MusicPlayerState.Stop;
        }

        public void AddTracksToPlaylist(IEnumerable<LavalinkTrack> tracks, string discordMemberName)
        {
            foreach (LavalinkTrack track in tracks)
            {
                AddATrackToPlaylist(track, discordMemberName);
            }
        }

        public void AddATrackToPlaylist(LavalinkTrack track, string discordMemberName)
        {
            if (track == null || string.IsNullOrEmpty(discordMemberName))
                return;

            _playlist.Add(new TrackData(track: track, discordMemberName: discordMemberName));
        }

        public TrackData GetCurrentTrack()
        {
            if (_currentTrackIndex >= _playlist.Count)
                return null;

            return _playlist[_currentTrackIndex];
        }

        public TrackData GetNextTrack()
        {
            if (_currentTrackIndex + 1 >= _playlist.Count)
                return null;

            _currentTrackIndex++;

            return _playlist[_currentTrackIndex];
        }

        public void ClearPlaylist()
        {
            _playlist.Clear();
            _currentTrackIndex = -1;
        }

        public async Task ShufflePlaylist()
        {
            await Task.Run(() =>
            {
                Random random = new();
                List<TrackData> shuffledPlaylist = new();

                int max = _playlist.Count;

                for (int i = 0; i < max; i++)
                {
                    int a = random.Next(0, _playlist.Count - shuffledPlaylist.Count);
                    shuffledPlaylist.Add(_playlist[a]);
                }

                _currentTrackIndex = -1;
            });
        }
    }
}