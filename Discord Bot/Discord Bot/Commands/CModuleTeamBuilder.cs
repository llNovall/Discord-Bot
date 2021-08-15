using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiny_Bot.Commands
{
    class CModuleTeamBuilder : BaseCommandModule
    {
        [Command("team")]
        [Description("Use this command to create teams. You can directly mention users or use their names.\nExample command would look like this: !team 2 user1 user2")]
        public async Task BuildTeam(CommandContext ctx, int numOfTeams, params string[] users)
        {
            if (users.Length <= 1)
            {
                await ctx.RespondAsync("Not enough members to build a team. At least 2 are required.");
                return;
            }

            if (numOfTeams == 1)
            {
                await ctx.RespondAsync("Invalid number of teams is provided. At least 2 are required.");
                return;
            }

            Dictionary<int, List<string>> teams = new Dictionary<int, List<string>>();

            for (int i = 0; i < numOfTeams; i++)
                teams.Add(i, new List<string>());

            Random random = new Random();
            List<string> members = users.ToList();

            int teamIndex = 0;

            for (int i = 0; i < members.Count; i++)
            {
                int num = random.Next(0, members.Count - 1);

                teams[teamIndex].Add(members[i]);
                members.RemoveAt(i);
                members = members.OrderBy(c => random.Next()).ToList();
                i--;

                teamIndex = teamIndex + 1 < numOfTeams ? teamIndex + 1 : 0;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Team Builder",
                Color = DiscordColor.Green
            };

            foreach (var item in teams)
            {
                embed.AddField($"Team {item.Key + 1}", $"{string.Join(", ", item.Value.Select(c => c))}");
            }

            await ctx.RespondAsync(embed);
        }

        [Command("team")]
        public async Task BuildTeam(CommandContext ctx, params string[] users)
        {
            int numOfTeams = 2;

            if (users.Length <= 1)
            {
                await ctx.RespondAsync("Not enough members to build a team. At least 2 are required.");
                return;
            }

            if (numOfTeams == 1)
            {
                await ctx.RespondAsync("Invalid number of teams is provided. At least 2 are required.");
                return;
            }

            Dictionary<int, List<string>> teams = new Dictionary<int, List<string>>();

            for (int i = 0; i < numOfTeams; i++)
                teams.Add(i, new List<string>());

            Random random = new Random();
            List<string> members = users.ToList();

            int teamIndex = 0;

            for (int i = 0; i < members.Count; i++)
            {
                int num = random.Next(0, members.Count - 1);

                teams[teamIndex].Add(members[i]);
                members.RemoveAt(i);
                members = members.OrderBy(c => random.Next()).ToList();
                i--;

                teamIndex = teamIndex + 1 < numOfTeams ? teamIndex + 1 : 0;
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Title = "Team Builder",
                Color = DiscordColor.Green
            };

            foreach (var item in teams)
            {
                embed.AddField($"Team {item.Key + 1}", $"{string.Join(", ", item.Value.Select(c => c))}");
            }

            await ctx.RespondAsync(embed);
        }
    }
}
