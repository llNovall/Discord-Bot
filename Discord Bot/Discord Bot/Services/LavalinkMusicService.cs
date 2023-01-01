using Discord_Bot.DataClasses;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System.Collections.Generic;

namespace Discord_Bot.Services
{
    internal class LavalinkMusicService
    {
        private Dictionary<ulong, MusicPlayerData> _musicPlayers;

        private Dictionary<DiscordGuild, Queue<LavalinkTrack>> _playlist;

        public LavalinkMusicService()
        {
            _musicPlayers = new Dictionary<ulong, MusicPlayerData>();
            _playlist = new Dictionary<DiscordGuild, Queue<LavalinkTrack>>();
        }

        public void AddTracksToMusicPlayer(ulong guildId, IEnumerable<LavalinkTrack> tracks, string discordMemberName)
        {
            if (!_musicPlayers.ContainsKey(guildId))
                _musicPlayers.Add(guildId, new MusicPlayerData());

            _musicPlayers[guildId].AddTracksToPlaylist(tracks, discordMemberName);
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
    }
}