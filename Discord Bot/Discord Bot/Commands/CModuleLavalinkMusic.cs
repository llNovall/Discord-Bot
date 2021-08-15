using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tiny_Bot.Commands.Enumerators;
using Tiny_Bot.Services;

namespace Tiny_Bot.Commands
{
    //[Group("Music")]
    //[Description("This is all the music commands I have at the moment. Feel free to let me know if you need more.")]
    class CModuleLavalinkMusic : BaseCommandModule
    {
        public LavalinkMusicService Data;
        public DiscordEmbedBuilderHelper EmbedHelper;

        [Command("join")]
        [Description("I'll join your voice channel.")]
        public async Task JoinChannel(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            if (!await SendErrorMessageIfMemberNotInAVoiceChannel(ctx))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = node.GetGuildConnection(ctx.Guild);

            if (connection != null)
            {
                if (connection.Channel == ctx.Member.VoiceState.Channel)
                {
                    DiscordEmbedBuilder embed1 = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                    embed1.WithTitle($"I'm already in channel __*{connection.Channel.Name}*__.");

                    await ctx.Channel.SendMessageAsync(embed1);
                    return;
                }
                else if(connection.Channel != null)
                {
                    await connection.DisconnectAsync();
                }
            }

           
            await node.ConnectAsync(ctx.Member.VoiceState.Channel);

            connection = node.GetGuildConnection(ctx.Guild);
            connection.PlaybackFinished += Connection_PlaybackFinished;

            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
            embed.WithTitle($"I am in channel __*{connection.Channel.Name}*__ now.");

            await ctx.Channel.SendMessageAsync(embed);
            return;
        }

        [Command("join")]
        public async Task JoinChannel(CommandContext ctx, DiscordChannel discordChannel)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            if (!await SendErrorMessageIfNotAValidVoiceChannel(ctx, discordChannel))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = lavalink.GetGuildConnection(ctx.Guild);

            if (connection != null)
            {
                if (connection.Channel == discordChannel)
                {
                    await ctx.Channel.SendMessageAsync($"I'm already in channel __*{discordChannel}*__.");
                    return;
                }
                else if (connection.Channel != null)
                {
                    await connection.DisconnectAsync();
                }
            }

            await node.ConnectAsync(discordChannel);

            connection = node.GetGuildConnection(ctx.Guild);
            connection.PlaybackFinished += Connection_PlaybackFinished;

            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
            embed.WithTitle($"I am in channel __*{connection.Channel.Name}*__ now.");

            await ctx.Channel.SendMessageAsync(embed);
        }
        

        [Command("leave")]
        [Description("I'll the voice channel that I'm in.")]
        public async Task LeaveChannel(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = node.GetGuildConnection(ctx.Guild);

            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
            
            if (connection != null)
            {
                string channelName = connection.Channel.Name;
                await connection.DisconnectAsync();
                embed.WithTitle($"I've left the channel __*{channelName}*__.");

                Data.ClearTracksFromPlaylist(ctx.Guild);
            }
            else
            {
                embed.WithTitle("I am not in a voice channel.");
            }

            await ctx.Channel.SendMessageAsync(embed);
        }


        [Command("play")]
        [Description("I can play song when provided with a search term or link.")]
        public async Task PlayMusic(CommandContext ctx, [RemainingText] string search)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = await JoinMemberChannel(ctx, node);

            if (connection == null)
                return;

            LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(search);

            if(loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($":no_entry: I could't find anything for __*{search}*__ :no_entry:");

                await ctx.Channel.SendMessageAsync(embed);

                return;
            }

            connection.PlaybackFinished -= Connection_PlaybackFinished;
            await PlayTrack(loadResult.Tracks.First(), connection, ctx);
            connection.PlaybackFinished += Connection_PlaybackFinished;
        }

        
        [Command("play")]
        public async Task PlayMusic(CommandContext ctx, [RemainingText] Uri url)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = await JoinMemberChannel(ctx, node);

            if (connection == null)
                return;

            LavalinkLoadResult loadResult = await node.Rest.GetTracksAsync(url);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches || loadResult.LoadResultType != LavalinkLoadResultType.TrackLoaded)
            {
                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($":no_entry:I've failed to load song from this link.:no_entry:");
                await ctx.Channel.SendMessageAsync(embed);
                return;
            }

            connection.PlaybackFinished -= Connection_PlaybackFinished;
            await PlayTrack(loadResult.Tracks.First(), connection, ctx);
            connection.PlaybackFinished += Connection_PlaybackFinished;
        }


        [Command("pause")]
        [Description("I can pause a song if it is being played.")]
        public async Task PauseMusic(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            if (!await SendErrorMessageIfMemberNotInAVoiceChannel(ctx))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = node.GetGuildConnection(ctx.Guild);

            if (!await SendErrorMessageIfBotNotInAVoiceChannel(ctx, connection))
                return;

            await UpdateMusicPlayState(ctx, connection, MusicCommandState.Pause);
        }

        [Command("resume")]
        [Description("I can resume playing songs.")]
        public async Task ResumeMusic(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);
            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            if (! await SendErrorMessageIfMemberNotInAVoiceChannel(ctx))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = node.GetGuildConnection(ctx.Guild);

            if (!await SendErrorMessageIfBotNotInAVoiceChannel(ctx, connection))
                return;

            if (!await SendErrorMessageIfMemberNotInSameChannelAsBot(ctx, connection))
                return;

            await UpdateMusicPlayState(ctx, connection, MusicCommandState.Resume);
        }

        [Command("skip")]
        [Aliases("stop")]
        [Description("I can skip current song.")]
        public async Task StopMusic(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);
            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            if (!await SendErrorMessageIfMemberNotInAVoiceChannel(ctx))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = node.GetGuildConnection(ctx.Guild);

            if (!await SendErrorMessageIfBotNotInAVoiceChannel(ctx, connection))
                return;

            if (!await SendErrorMessageIfMemberNotInSameChannelAsBot(ctx, connection))
                return;

            await UpdateMusicPlayState(ctx, connection, MusicCommandState.Stop);
        }

        [Command("pl")]
        [Description("I can play from a long list of songs when provided with a playlist link.")]
        public async Task PlayPlaylist(CommandContext ctx, Uri url)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();

            LavalinkGuildConnection connection = await JoinMemberChannel(ctx, node);

            if (connection == null)
                return;

            if (!await SendErrorMessageIfBotNotInAVoiceChannel(ctx, connection))
                return;

            LavalinkLoadResult result = await connection.GetTracksAsync(url);

            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);

            if(result.LoadResultType == LavalinkLoadResultType.LoadFailed || result.LoadResultType != LavalinkLoadResultType.PlaylistLoaded)
            {
                embed.WithTitle(":no_entry:I've failed to load the playlist:no_entry:");

                await ctx.Channel.SendMessageAsync(embed);
                return;
            }
            
            embed.WithTitle($"I've loaded playlist __*{result.PlaylistInfo.Name}*__ with __*{result.Tracks.Count()}*__ tracks.");

            await ctx.Channel.SendMessageAsync(embed);

            await Data.AddTracksToPlaylist(ctx.Guild, result.Tracks);

            if(connection.CurrentState.CurrentTrack == null)
            {
                LavalinkTrack track = Data.GetNextTrack(ctx.Guild);

                
                if (track == null)
                {
                    embed.WithTitle("`I'm out of songs to play...`");
                    await ctx.Channel.SendMessageAsync(embed);
                    return;
                }

                await PlayTrack(track, connection, ctx);
            }
        }

        [Command("queue")]
        [Description("I can show you all the songs in the queue.")]
        public async Task ShowQueue(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = lavalink.GetGuildConnection(ctx.Guild);

            if (! await SendErrorMessageIfBotNotInAVoiceChannel(ctx, connection))
                return;

            List<LavalinkTrack> tracks = Data.GetDefinedTracksFromPlaylist(connection.Guild, 10);

            string playlistInfo = "```";

            if (tracks.Count > 0)
                tracks.ForEach(c => playlistInfo += $"{tracks.IndexOf(c) + 1}) {c.Title} - {c.Length:g}.\n");
            else
                playlistInfo += "I have nothing left to play...";

            playlistInfo += "```";
            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);

            embed.AddField("Currently Playing", connection.CurrentState.CurrentTrack != null ? $"```{connection.CurrentState.CurrentTrack.Title}```" : "```I'm playing silence right now...```");
            embed.AddField("Queue Information", playlistInfo, true);

            await ctx.Channel.SendMessageAsync(embed);
        }
       

        [Command("np")]
        [Description("I will let you know about the current song being played.")]
        public async Task ShowCurrentTrack(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = lavalink.GetGuildConnection(ctx.Guild);

            if (! await SendErrorMessageIfBotNotInAVoiceChannel(ctx, connection))
                return;

            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);

            if (connection.CurrentState.CurrentTrack != null)
                embed.AddField("Now Playing", $"`{connection.CurrentState.CurrentTrack.Title}`");
            else
                embed.WithTitle("`I'm playing silence right now...`");

            await ctx.Channel.SendMessageAsync(embed);
        }

        [Command("clear")]
        [Description("I will remove everything from my playlist...")]
        public async Task ClearPlaylist(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            if (!await SendErrorMessageIfMemberNotInAVoiceChannel(ctx))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = node.GetGuildConnection(ctx.Guild);

            if (!await SendErrorMessageIfBotNotInAVoiceChannel(ctx, connection))
                return;

            if (!await SendErrorMessageIfMemberNotInSameChannelAsBot(ctx, connection))
                return;

            Data.ClearTracksFromPlaylist(connection.Guild);

            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
            embed.WithTitle($"```I've removed everything from the playlist...```");

            await ctx.Channel.SendMessageAsync(embed);
        }

        [Command("shuffle")]
        [Description("I will shuffle everything in the playlist.")]
        public async Task ShufflePlaylist(CommandContext ctx)
        {
            LavalinkExtension lavalink = await GetLavalink(ctx);

            if (lavalink == null)
                return;

            if (!await SendErrorMessageIfNodesNotAvailable(ctx, lavalink))
                return;

            if (!await SendErrorMessageIfMemberNotInAVoiceChannel(ctx))
                return;

            LavalinkNodeConnection node = lavalink.GetIdealNodeConnection();
            LavalinkGuildConnection connection = node.GetGuildConnection(ctx.Guild);

            if (!await SendErrorMessageIfBotNotInAVoiceChannel(ctx, connection))
                return;

            if (!await SendErrorMessageIfMemberNotInSameChannelAsBot(ctx, connection))
                return;

            await Data.ShufflePlaylist(connection.Guild);

            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
            embed.WithTitle($"I've shuffled everything in the playlist.");

            await ctx.Channel.SendMessageAsync(embed);
        }
        private async Task<LavalinkExtension> GetLavalink(CommandContext ctx)
        {
            LavalinkExtension lavalink = ctx.Client.GetLavalink();

            if (lavalink == null)
            {
                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($":no_entry: Lavalink is not configured :no_entry:");

                await ctx.Channel.SendMessageAsync(embed);
            }

            return lavalink;
        }

        private bool IsMemberInVoiceChannel(CommandContext ctx)
        {
            if (ctx.Member?.VoiceState?.Channel == null)
                return false;

            return true;
        }

        private bool IsMemberInSameVoiceChannelAsBot(CommandContext ctx, LavalinkGuildConnection connection)
        {
            if(connection != null && ctx != null)
            {
                if (IsMemberInVoiceChannel(ctx))
                {
                    if (ctx.Member.VoiceState.Channel == connection.Channel)
                        return true;
                }
            }

            return false;
        }
        private async Task<bool> SendErrorMessageIfMemberNotInAVoiceChannel(CommandContext ctx)
        {
            if (!IsMemberInVoiceChannel(ctx))
            {
                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($":no_entry: You are not in a voice channel :no_entry:");
                await ctx.Channel.SendMessageAsync(embed);
                return false;
            }

            return true;
        }

        private async Task<bool> SendErrorMessageIfNotAValidVoiceChannel(CommandContext ctx, DiscordChannel discordChannel)
        {
            if(discordChannel.Type != DSharpPlus.ChannelType.Voice)
            {
                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($":no_entry: This is not a valid voice channel. :no_entry:");
                await ctx.Channel.SendMessageAsync(embed);

                return false;
            }

            return true;
        }
        private async Task<bool> SendErrorMessageIfNodesNotAvailable(CommandContext ctx, LavalinkExtension lavalink)
        {
            if (!lavalink.ConnectedNodes.Any())
            {
                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($":no_entry: I'm unable to establish any connection with lavalink nodes. :no_entry:");
                await ctx.Channel.SendMessageAsync(embed);

                return false;
            }

            return true;
        }

        private async Task<bool> SendErrorMessageIfMemberNotInSameChannelAsBot(CommandContext ctx, LavalinkGuildConnection connection)
        {
            if (!IsMemberInSameVoiceChannelAsBot(ctx, connection))
            {
                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($":no_entry: You have to be in the channel __*{ctx.Channel.Name}*__ to use this command :no_entry:");
                
                await ctx.Channel.SendMessageAsync(embed);

                return false;
            }

            return true;
        }

        private async Task<bool> SendErrorMessageIfBotNotInAVoiceChannel(CommandContext ctx, LavalinkGuildConnection connection)
        {
            if (connection == null)
            {
                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($":no_entry: I'm not in a voice channel (Use *join* command) :no_entry:");

                await ctx.Channel.SendMessageAsync(embed);
                return false;
            }

            return true;
        }

        private async Task UpdateMusicPlayState(CommandContext ctx, LavalinkGuildConnection connection, MusicCommandState state)
        {
            if (connection != null && ctx.Member.VoiceState.Channel == connection.Channel)
            {
                string message = "";

                if (ctx.Member.VoiceState.Channel == connection.Channel)
                {
                    if (connection.CurrentState.CurrentTrack != null)
                    {
                        

                        switch (state)
                        {
                    
                            case MusicCommandState.Pause:
                                await connection.PauseAsync();
                                message = $"I've paused the track __*{connection.CurrentState.CurrentTrack.Title}*__.";
                                break;
                            case MusicCommandState.Resume:
                                await connection.ResumeAsync();
                                message = $"I've resumed the track __*{connection.CurrentState.CurrentTrack.Title}*__.";

                                break;
                            case MusicCommandState.Stop:
                                await connection.StopAsync();
                                message = $"I've skipped the track __*{connection.CurrentState.CurrentTrack.Title}*__.";
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        message = "`I'm out of songs to play...`";
                    }

                }
                else
                {
                    switch (state)
                    {
                        
                        case MusicCommandState.Pause:
                            message = $"You must be in the channel __*{connection.Channel.Name}*__ to play.";
                            break;
                        case MusicCommandState.Resume:
                            message = $"You must be in the channel __*{connection.Channel.Name}*__ to resume.";
                            break;
                        case MusicCommandState.Stop:
                            message = $"You must be in the channel __*{connection.Channel.Name}*__ to stop.";
                            break;
                        default:
                            break;
                    }
                }

                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle(message);
                await ctx.Channel.SendMessageAsync(embed);
            }
        }

        private async Task<LavalinkGuildConnection> JoinMemberChannel(CommandContext ctx, LavalinkNodeConnection node)
        {
            if (! await SendErrorMessageIfMemberNotInAVoiceChannel(ctx))
                return null;

            LavalinkGuildConnection connection = node.GetGuildConnection(ctx.Guild);

            if (connection?.Channel != null && ctx.Member.VoiceState?.Channel != null)
            {
                if(connection.Channel != ctx.Member.VoiceState.Channel)
                {
                    await connection.DisconnectAsync();
                    connection = null;
                }
            }

            if (connection == null)
            {
                await node.ConnectAsync(ctx.Member.VoiceState.Channel);

                connection = node.GetGuildConnection(ctx.Guild);
                connection.PlaybackFinished += Connection_PlaybackFinished;

                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.WithTitle($"I've joined __*{connection.Channel.Name}*__");
                await ctx.Channel.SendMessageAsync(embed);
            }

            return connection;
        }

        private async Task PlayTrack(LavalinkTrack track, LavalinkGuildConnection connection, CommandContext ctx)
        {
            if (connection != null && ctx != null && track != null)
            {
                await connection.PlayAsync(track);

                DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(ctx.Client);
                embed.AddField("Now Playing", $"`{track.Title}!`");

                await ctx.Channel.SendMessageAsync(embed);
            }
        }

        private async Task Connection_PlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs e)
        {
            LavalinkTrack track = Data.GetNextTrack(sender.Guild);

            DiscordChannel discordChannel = sender.Guild.Channels.Where(c => c.Value.Type == DSharpPlus.ChannelType.Text).FirstOrDefault().Value;

            DiscordEmbedBuilder embed = EmbedHelper.GetDefaultEmbedTemplate(sender.Node.Discord);
  
            if (track == null)
            {
                embed.WithTitle("`I'm out of songs to play...`");

                await discordChannel.SendMessageAsync(embed);
                return;
            }

            await sender.PlayAsync(track);

            embed.AddField("Now Playing", $"`{track.Title}`");
            await discordChannel.SendMessageAsync(embed);
        }
    }
}
