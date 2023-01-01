using DSharpPlus.Lavalink;

namespace Discord_Bot.DataClasses
{
    public class TrackData
    {
        public LavalinkTrack Track;
        public string DiscordMemberName;

        public TrackData(LavalinkTrack track, string discordMemberName)
        {
            Track = track;
            DiscordMemberName = discordMemberName;
        }
    }
}