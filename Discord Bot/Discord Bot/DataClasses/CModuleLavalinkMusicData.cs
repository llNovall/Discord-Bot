using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Tiny_Bot.DataClasses
{
    class CModuleLavalinkMusicData
    {
        private Dictionary<DiscordGuild, Queue<LavalinkTrack>> _playlist;

        public CModuleLavalinkMusicData()
        {
            _playlist = new Dictionary<DiscordGuild, Queue<LavalinkTrack>>();
        }

        public async Task AddTracksToPlaylist(DiscordGuild guild, IEnumerable<LavalinkTrack> tracksToAdd)
        {
            if (!_playlist.ContainsKey(guild))
                _playlist.Add(guild, new Queue<LavalinkTrack>());

            await Task.Run(() => tracksToAdd.ToList().ForEach(c => _playlist[guild].Enqueue(c)));
        }
        public async Task AddTracksToPlaylist(DiscordGuild guild, LavalinkTrack trackToAdd)
        {
            if (!_playlist.ContainsKey(guild))
                _playlist.Add(guild, new Queue<LavalinkTrack>());

            await Task.Run(() => _playlist[guild].Enqueue(trackToAdd));
        }

        public void ClearTracksFromPlaylist(DiscordGuild guild)
        {
            if (_playlist.ContainsKey(guild))
                _playlist[guild].Clear();
        }

        public async Task ShufflePlaylist(DiscordGuild guild)
        {
            if (_playlist.ContainsKey(guild))
            {
                Random random = new Random();

                Queue<LavalinkTrack> shuffledQueue = new Queue<LavalinkTrack>();
                List<LavalinkTrack> currentList = _playlist[guild].ToList();

                await Task.Run(() =>
                {
                    currentList = currentList.OrderBy(c => random.Next()).ToList();

                    currentList.ForEach(c => shuffledQueue.Enqueue(c));

                    _playlist[guild] = shuffledQueue;
                });
            }
        }

        public LavalinkTrack GetNextTrack(DiscordGuild guild) 
            => _playlist.ContainsKey(guild) ? _playlist[guild].Any() ? _playlist[guild].Dequeue() : null : null;

        public Queue<LavalinkTrack> GetQueue(DiscordGuild guild) 
            => _playlist.ContainsKey(guild) ? _playlist[guild] : new Queue<LavalinkTrack>();

        public List<LavalinkTrack> GetDefinedTracksFromPlaylist(DiscordGuild guild, int numOfTracks)
        {
            if (!_playlist.ContainsKey(guild))
                return new List<LavalinkTrack>();

            int count = 0;

            List<LavalinkTrack> tracksToReturn = new List<LavalinkTrack>();

            foreach (var item in _playlist[guild])
            {
                tracksToReturn.Add(item);
                count++;
                if (count == numOfTracks)
                    break;
            }

            return tracksToReturn;
        }
    }
}
