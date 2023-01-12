using Discord_Bot.Database;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot.Services
{
    internal class RolesFinder
    {
        public DatabaseManager DatabaseManager;

        public RolesFinder(DatabaseManager databaseManager)
        {
            DatabaseManager = databaseManager;
        }

        public async Task<DiscordRole> GetRoleIDForRules(CommandContext ctx)
        {
            Dictionary<string, ulong> result = await DatabaseManager.GetRoleForGuildRules(ctx.Guild.Id);

            if (!result.ContainsKey("role_id"))
                return null;

            ulong roleID = result["role_id"];

            DiscordRole role = ctx.Guild.GetRole(roleID);

            return role;
        }

        public async Task<DiscordRole> GetRoleIDForRules(DiscordGuild guild)
        {
            Dictionary<string, ulong> result = await DatabaseManager.GetRoleForGuildRules(guild.Id);

            if (!result.ContainsKey("role_id"))
                return null;

            ulong roleID = result["role_id"];

            DiscordRole role = guild.GetRole(roleID);

            return role;
        }
    }
}