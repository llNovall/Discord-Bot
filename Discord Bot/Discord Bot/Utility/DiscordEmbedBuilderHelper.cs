using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tiny_Bot.Utility
{
    public class DiscordEmbedBuilderHelper
    {
        private DiscordEmbedBuilder _defaultEmbed;

        public DiscordEmbedBuilderHelper()
        {
            _defaultEmbed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
            };
        }

        public DiscordEmbedBuilder GetDefaultEmbedTemplate(DiscordClient discordClient)
            => new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Magenta,
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = discordClient.CurrentUser.Username,
                    IconUrl = discordClient.CurrentUser.AvatarUrl,
                }
            };
        public DiscordEmbedBuilder BuildEmbedContent(string title, string description, DiscordUser user = null)
        {
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(_defaultEmbed)
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    Name = user?.Username
                },
                Title = title,
                Description = description,
            };

            //embed.AddField(description);

            return embed;
        }
    }
}
