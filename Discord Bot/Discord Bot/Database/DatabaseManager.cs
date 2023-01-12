using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Discord_Bot.Database
{
    public class DatabaseManager
    {
        private string _connectionString { get; }

        public DatabaseManager()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["default_connection"].ToString();
        }

        public async Task<GuildChannelUsageData> GetGuildChannelUsageData(ulong guildId, string channel_usage_type)
        {
            string query = "SELECT [guild_id], [channel_usage_type], [channel_id] FROM [DbDiscord].[dbo].[TbGuildChannelUsage] WHERE [guild_id] = @GuildId AND [channel_usage_type] = @Channel_usage_type";

            GuildChannelUsageData data = new GuildChannelUsageData();

            try
            {
                using (SqlConnection myConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                    {
                        oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;
                        oCmd.Parameters.Add("@Channel_usage_type", System.Data.SqlDbType.VarChar).Value = channel_usage_type;

                        await myConnection.OpenAsync();

                        using (SqlDataReader oReader = await oCmd.ExecuteReaderAsync())
                        {
                            while (await oReader.ReadAsync())
                            {
                                data.GuildId = Convert.ToUInt64(oReader["guild_id"]);
                                data.ChannelUsageType = oReader["channel_usage_type"].ToString();
                                data.ChannelId = Convert.ToUInt64(oReader["channel_id"]);
                            }

                            myConnection.Close();
                        }
                    }

                    await myConnection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return data;
        }

        public async Task<bool> UpdateChannelUsageForChannel(ulong guildId, ulong channelId, string channel_usage_type)
        {
            try
            {
                string query = "IF NOT EXISTS (SELECT * FROM DbDiscord.dbo.TbGuild WHERE guild_id = @GuildId) INSERT INTO DbDiscord.dbo.TbGuild (guild_id) VALUES (@GuildId) IF NOT EXISTS (SELECT * FROM DbDiscord.dbo.TbChannelUsageType WHERE channel_usage_type = @ChannelUsageType) INSERT INTO DbDiscord.dbo.TbChannelUsageType(channel_usage_type) VALUES (@ChannelUsageType) UPDATE DbDiscord.dbo.TbGuildChannelUsage set channel_id = @ChannelId WHERE guild_id = @GuildId AND channel_usage_type = @ChannelUsageType IF @@ROWCOUNT=0 INSERT INTO DbDiscord.dbo.TbGuildChannelUsage (guild_id, channel_usage_type, channel_id) values(@GuildId, @ChannelUsageType, @ChannelId)";

                using (SqlConnection myConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                    {
                        oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;
                        oCmd.Parameters.Add("@ChannelId", System.Data.SqlDbType.BigInt).Value = channelId;
                        oCmd.Parameters.Add("@ChannelUsageType", System.Data.SqlDbType.VarChar).Value = channel_usage_type;

                        await myConnection.OpenAsync();

                        await oCmd.ExecuteNonQueryAsync();
                    }

                    await myConnection.CloseAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }

        public async Task<Dictionary<string, ulong>> GetRoleForGuildRules(ulong guildId)
        {
            string query = "SELECT [guild_id], [role_id] FROM [DbDiscord].[dbo].[TbRules] WHERE [guild_id] = @GuildId";

            Dictionary<string, ulong> data = new Dictionary<string, ulong>();

            try
            {
                using (SqlConnection myConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                    {
                        oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;

                        await myConnection.OpenAsync();

                        using (SqlDataReader oReader = await oCmd.ExecuteReaderAsync())
                        {
                            while (await oReader.ReadAsync())
                            {
                                data.Add("guild_id", Convert.ToUInt64(oReader["guild_id"]));
                                data.Add("role_id", Convert.ToUInt64(oReader["role_id"]));
                            }

                            myConnection.Close();
                        }
                    }

                    await myConnection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return data;
        }

        public async Task<bool> UpdateRoleForGuildRules(ulong guildId, ulong roleId)
        {
            try
            {
                string query = "IF NOT EXISTS (SELECT * FROM DbDiscord.dbo.TbGuild WHERE guild_id = @GuildId) INSERT INTO DbDiscord.dbo.TbGuild (guild_id) VALUES (@GuildId) UPDATE DbDiscord.dbo.TbRules set role_id = @RoleId WHERE guild_id = @GuildId IF @@ROWCOUNT=0 INSERT INTO DbDiscord.dbo.TbRules (guild_id, role_id) values(@GuildId, @RoleId)";

                using (SqlConnection myConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                    {
                        oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;
                        oCmd.Parameters.Add("@RoleId", System.Data.SqlDbType.BigInt).Value = roleId;

                        await myConnection.OpenAsync();

                        await oCmd.ExecuteNonQueryAsync();
                    }

                    await myConnection.CloseAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return false;
        }
    }
}