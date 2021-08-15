using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
namespace Tiny_Bot.Commands
{
    class CModuleVoice : BaseCommandModule
    {
        [Command("join")]
        public async Task JoinChannel(CommandContext ctx)
        {
            VoiceNextExtension voiceNext = ctx.Client.GetVoiceNext();

            if(voiceNext == null)
            {
                await ctx.Channel.SendMessageAsync("Voice Next not configured.");
                return;
            }

            VoiceNextConnection connection = voiceNext.GetConnection(ctx.Guild);

            if(connection != null)
            {
                await ctx.Channel.SendMessageAsync("Already in a voice channel.");
                return;
            }

            DiscordVoiceState voiceState = ctx.Member.VoiceState;

            if(ctx.Member?.VoiceState?.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("You are not in a voice channel.");
                return;
            }

            await voiceNext.ConnectAsync(ctx.Member.VoiceState.Channel);
            await ctx.Channel.SendMessageAsync($"Joined {ctx.Member.VoiceState.Channel.Name}");
            return;
        }

        [Command("leave")]
        public async Task LeaveChannel(CommandContext ctx)
        {
            VoiceNextExtension voiceNext = ctx.Client.GetVoiceNext();

            if (voiceNext == null)
            {
                await ctx.Channel.SendMessageAsync("Voice Next not configured.");
                return;
            }

            VoiceNextConnection connection = voiceNext.GetConnection(ctx.Guild);

            if (connection != null)
            {
                connection.Disconnect();
                await ctx.Channel.SendMessageAsync("Left the voice channel.");
                return;
            }
            else
                await ctx.Channel.SendMessageAsync("Not in a voice channel.");
        }

    }
}
