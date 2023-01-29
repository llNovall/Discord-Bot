using Discord_Bot.Services;
using Discord_Bot.Services.DataClasses;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    [Category("Music")]
    internal class CModuleLavalinkMusic : CModuleEnableable
    {
        public LavalinkMusicService LavalinkMusicService;
        public SpotifyService SpotifyService;

        public CModuleLavalinkMusic()
        {
            _serviceName = "music";

            Initialize().GetAwaiter();
        }

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
            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", discordGuild);

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

        [Command("enablemusic")]
        [Description("Enable music.\nUsage - **[Prefix]enablemusic [Channel]**")]
        public async Task EnableMusicAsync(CommandContext ctx, DiscordChannel musicChannel)
        {
            await EnableModuleAsync(ctx, musicChannel);
        }

        [Command("disablemusic")]
        [Description("Disables music.\nUsage - **[Prefix]disablemusic**")]
        public async Task DisableMusicAsync(CommandContext ctx)
        {
            await DisableModuleAsync(ctx);
        }

        [Command("join")]
        [Description("Joins your current channel.\nUsage - **[Prefix]join**")]
        public async Task JoinChannelAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.JoinChannelAsync(ctx.Client, ctx.Guild, ctx.Member);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("leave")]
        [Description("Leaves the channel the bot is in currently.\nUsage - **[Prefix]leave**")]
        public async Task LeaveChannelAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.LeaveChannelAsync(ctx.Guild, ctx.Member);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("queue")]
        [Description("Pass a search term, youtube or spotify link to add songs to the playlist.\nUsage - **[Prefix]queue [search / link]**")]
        public async Task QueueSongAsync(CommandContext ctx, [RemainingText] string search)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.QueueSongAsync(ctx.Guild, ctx.Member, search);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("pause")]
        [Description("Pauses the music player.\nUsage - **[Prefix]pause**")]
        public async Task PauseSongAsync(CommandContext ctx)
        {
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
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            var status = await LavalinkMusicService.PauseSongAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("resume")]
        [Description("Resumes the music player.\nUsage - **[Prefix]resume**")]
        public async Task ResumeSongAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.ResumeSongAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("stop"), Aliases("skip")]
        [Description("Skips current song.\nUsage - **[Prefix]stop**")]
        public async Task StopSongAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.StopSongAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("clear")]
        [Description("Clears current playlist.\nUsage - **[Prefix]clear**")]
        public async Task ClearPlaylistAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = LavalinkMusicService.ClearPlaylistAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("shuffle")]
        [Description("Shuffles current playlist.\nUsage - **[Prefix]shuffle**")]
        public async Task ShufflePlaylistAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.ShufflePlaylistAsync(ctx.Guild);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("showqueue")]
        [Description("Displays all the songs in current playlist.\nUsage - **[Prefix]showqueue**")]
        public async Task ShowQueueAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.ShowQueueAsync(ctx.Guild, musicChannel, ctx.Member);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }

        [Command("np")]
        [Description("Displays the current song.\nUsage - **[Prefix]np**")]
        public async Task ShowNowPlayingAsync(CommandContext ctx)
        {
            DiscordChannel adminChannel = GuildManager.GetChannelFor("admin", ctx);
            if (adminChannel == null)
                return;

            DiscordChannel musicChannel = GuildManager.GetChannelFor("music", ctx);

            if (musicChannel == null)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, adminChannel, Helper.MessageSeverity.Negative, "Failed to find music channel.");
                return;
            }

            if (!IsServiceEnabled(ctx.Guild.Id))
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, musicChannel, Helper.MessageSeverity.Negative,
                    "Please enable music by using command **enablemusic**");
                return;
            }

            if (musicChannel != ctx.Channel)
            {
                await Helper.SendMessageToChannelAsync(ctx.Client, ctx.Channel, Helper.MessageSeverity.Negative, $"Use the channel {musicChannel.Name} to execute this command.");
                return;
            }

            var status = await LavalinkMusicService.ShowNowPlayingAsync(ctx.Guild, musicChannel);

            await SendStatusMessage(ctx.Client, musicChannel, status);
        }
    }
}