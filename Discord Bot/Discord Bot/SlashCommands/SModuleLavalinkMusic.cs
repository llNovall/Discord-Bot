using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Threading.Tasks;

namespace Discord_Bot.SlashCommands
{
    [SlashCommandGroup("Music", "Commands for all of your music needs.")]
    public class SModuleLavalinkMusic : ApplicationCommandModule
    {
        public LavalinkMusicService LavalinkMusicService;
        public Helper Helper;
        public GuildManager GuildManager;

        private async Task SendStatusMessage(BaseDiscordClient client, DiscordChannel musicChannel, LavalinkMusicService.Status status)
        {
            switch (status)
            {
                case LavalinkMusicService.Status.None:
                    break;

                case LavalinkMusicService.Status.LavalinkNotFound:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Negative, $"Lavalink Service not found.");
                    break;

                case LavalinkMusicService.Status.NodeNotFound:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Negative, $"No valid nodes found.");
                    break;

                case LavalinkMusicService.Status.UserNotInVoiceChannel:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Negative, $"You aren't in a voice channel.");
                    break;

                case LavalinkMusicService.Status.UserNotInSameChannelAsBot:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Negative, $"You aren't in the same voice channel as the bot.");
                    break;

                case LavalinkMusicService.Status.UserInSameChannelAsBot:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Neutral, $"I'm already there with you.");
                    break;

                case LavalinkMusicService.Status.BotLeftVoiceChannel:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Positive, $"I've left the voice channel.");
                    break;

                case LavalinkMusicService.Status.BotNotInVoiceChannel:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Negative, $"I'm not in voice channel. Use the command join to make me join your channel.");
                    break;

                case LavalinkMusicService.Status.NoTracksFound:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Negative, $"I've failed to find any tracks.");
                    break;

                case LavalinkMusicService.Status.JoinedVoiceChannel:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Positive, $"I've joined your voice channel.");
                    break;

                case LavalinkMusicService.Status.PlaylistCleared:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Positive, $"I've cleared the playlist.");
                    break;

                case LavalinkMusicService.Status.PlaylistShuffled:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Positive, $"I've shuffled the playlist.");
                    break;

                case LavalinkMusicService.Status.QueueSuccessful:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Positive, $"I've added songs to the queue.");
                    break;

                case LavalinkMusicService.Status.PlayerPaused:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Positive, $"I've paused the player.");
                    break;

                case LavalinkMusicService.Status.PlayerResumed:
                    await Helper.SendMessageToChannelAsync(client, musicChannel, Helper.MessageSeverity.Positive, $"I've resumed the player.");
                    break;

                default:
                    break;
            }
        }

        [SlashCommand("join", "Bot will join your current voice channel")]
        public async Task JoinChannelAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.JoinChannelAsync(ctx.Client, ctx.Guild, ctx.Member);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("leave", "Leave the current voice channel that the bot is in.")]
        public async Task LeaveChannelAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.LeaveChannelAsync(ctx.Guild, ctx.Member);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("pause", "Pauses the music player")]
        public async Task PauseSongAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.PauseSongAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("resume", "Resumes the music player.")]
        public async Task ResumeSongAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.ResumeSongAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("stop", "Stop the music player.")]
        public async Task StopSongAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.StopSongAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("clear", "Clears the playlist.")]
        public async Task ClearPlaylistAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = LavalinkMusicService.ClearPlaylistAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("shuffle", "Shuffles the playlist.")]
        public async Task ShufflePlaylistAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.ShufflePlaylistAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("showqueue", "Shows the songs in current queue.")]
        public async Task ShowQueueAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.ShowQueueAsync(ctx.Guild, musicChannel, ctx.Member);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("np", "Shows the song that is currently playing.")]
        public async Task ShowNowPlayingAsync(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.ShowNowPlayingAsync(ctx.Guild, musicChannel);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }

        [SlashCommand("queue", "Pass a search term to search for a song.")]
        public async Task QueueSongAsync(InteractionContext ctx, [Option("SearchName", "Pass a search term, youtube or spotify link.")][RemainingText] string search)
        {
            await ctx.DeferAsync(true);

            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.QueueSongAsync(ctx.Guild, ctx.Member, search);

            await SendStatusMessage(ctx.Client, musicChannel, status);
            await ctx.DeleteResponseAsync();
        }
    }
}