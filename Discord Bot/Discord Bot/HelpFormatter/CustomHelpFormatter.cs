﻿using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Discord_Bot.HelpFormatter
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder _embed;

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            _embed = new DiscordEmbedBuilder();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _embed.AddField(command.Name, command.Description);

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            Dictionary<string, List<Command>> commandsDict = new();

            foreach (var cmd in cmds)
            {
                if (string.IsNullOrWhiteSpace(cmd.Description))
                    continue;
                if (cmd.Category == null)
                    continue;

                if (commandsDict.ContainsKey(cmd.Category))
                    commandsDict[cmd.Category].Add(cmd);
                else
                    commandsDict.Add(cmd.Category, new() { cmd });
                _embed.AddField(cmd.Name, cmd.Description);
            }

            DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder().WithContent("Help");

            foreach (var item in commandsDict)
            {
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder().WithColor(DiscordColor.Chartreuse);
                embed.WithTitle(item.Key);

                foreach (var cmd in item.Value)
                {
                    embed.AddField(cmd.Name, cmd.Description);
                }

                messageBuilder.AddEmbed(embed);
            }

            this.Context.Channel.SendMessageAsync(messageBuilder);

            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage();
            //return new CommandHelpMessage(embed: _embed);
        }
    }
}