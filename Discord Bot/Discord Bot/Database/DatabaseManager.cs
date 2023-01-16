using DSharpPlus;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace Discord_Bot.Database
{
    public class DatabaseManager
    {
        private ILogger<BaseDiscordClient> _logger;
        private string _connectionString { get; }

        public DatabaseManager(ILogger<BaseDiscordClient> logger)
        {
            _logger = logger;
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
                _logger.Log(LogLevel.Error, message: $"Failed to retreive values for channel usage {channel_usage_type} for guild {guildId}.\n Error : {e.Message}");
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
                _logger.Log(LogLevel.Error, message: $"Failed to update values for channel usage for channel {channelId} for guild {guildId}.\n Error : {e.Message}");
            }

            return false;
        }

        public async Task<Dictionary<string, ulong>> GetChannelUsageDataForGuild(ulong guildId)
        {
            string query = "SELECT [channel_usage_type], [channel_id] FROM [DbDiscord].[dbo].[TbGuildChannelUsage] WHERE [guild_id] = @GuildId";

            Dictionary<string, ulong> data = new();

            try
            {
                using SqlConnection myConnection = new SqlConnection(_connectionString);
                using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                {
                    oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;

                    await myConnection.OpenAsync();

                    using SqlDataReader oReader = await oCmd.ExecuteReaderAsync();
                    while (await oReader.ReadAsync())
                    {
                        data.Add(oReader["channel_usage_type"].ToString(), Convert.ToUInt64(oReader["channel_id"]));
                    }

                    myConnection.Close();
                }

                await myConnection.CloseAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, message: $"Failed to retreive values for guild {guildId}.\n Error : {e.Message}");
            }

            return data;
        }

        public async Task<ulong> GetRoleForGuild(ulong guildId, string roleUsage)
        {
            string query = "SELECT [role_id] FROM [DbDiscord].[dbo].[TbGuildRoles] WHERE [guild_id] = @GuildId AND [role_usage] = @RoleUsage";

            ulong roleId = 0;

            try
            {
                using (SqlConnection myConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                    {
                        oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;
                        oCmd.Parameters.Add("@RoleUsage", System.Data.SqlDbType.VarChar).Value = roleUsage;

                        await myConnection.OpenAsync();

                        roleId = Convert.ToUInt64(await oCmd.ExecuteScalarAsync());
                    }

                    await myConnection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, message: $"Failed to retreive values for role for {roleUsage} in guild {guildId}.\n Error : {e.Message}");
            }

            return roleId;
        }

        public async Task<Dictionary<string, ulong>> GetRolesForGuild(ulong guildId)
        {
            string query = "SELECT [role_id], [role_usage] FROM [DbDiscord].[dbo].[TbGuildRoles] WHERE [guild_id] = @GuildId";

            Dictionary<string, ulong> rolesDict = new Dictionary<string, ulong>();

            try
            {
                using (SqlConnection myConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                    {
                        oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;

                        await myConnection.OpenAsync();

                        using SqlDataReader dataReader = await oCmd.ExecuteReaderAsync();
                        while (await dataReader.ReadAsync())
                        {
                            rolesDict.Add(dataReader["role_usage"].ToString(),
                                        Convert.ToUInt64(dataReader["role_id"]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, message: $"Failed to retrieve roles for guild {guildId}.\n Error : {e.Message}");
            }

            return rolesDict;
        }

        public async Task<bool> UpdateRoleForGuild(ulong guildId, ulong roleId, string roleUsage)
        {
            try
            {
                string query = "IF NOT EXISTS (SELECT * FROM DbDiscord.dbo.TbGuild WHERE guild_id = @GuildId) INSERT INTO DbDiscord.dbo.TbGuild (guild_id) VALUES (@GuildId) UPDATE DbDiscord.dbo.TbGuildRoles set role_id = @RoleId WHERE guild_id = @GuildId AND role_usage = @RoleUsage IF @@ROWCOUNT=0 INSERT INTO DbDiscord.dbo.TbGuildRoles (guild_id, role_id, role_usage) values(@GuildId, @RoleId, @RoleUsage)";

                using (SqlConnection myConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                    {
                        oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;
                        oCmd.Parameters.Add("@RoleId", System.Data.SqlDbType.BigInt).Value = roleId;
                        oCmd.Parameters.Add("@RoleUsage", System.Data.SqlDbType.VarChar).Value = roleUsage;

                        await myConnection.OpenAsync();

                        await oCmd.ExecuteNonQueryAsync();
                    }

                    await myConnection.CloseAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, message: $"Failed to update values for roles for guild rules for guild {guildId}.\n Error : {e.Message}");
            }

            return false;
        }

        public async Task<bool> UpdateLoggerStatusForGuild(ulong guildId, bool isEnabled)
        {
            try
            {
                string query = "IF NOT EXISTS (SELECT * FROM DbDiscord.dbo.TbGuild WHERE guild_id = @GuildId) INSERT INTO DbDiscord.dbo.TbGuild (guild_id) VALUES (@GuildId) UPDATE DbDiscord.dbo.TbLogger set is_enabled = @IsEnabled WHERE guild_id = @GuildId IF @@ROWCOUNT=0 INSERT INTO DbDiscord.dbo.TbLogger (guild_id, is_enabled) values(@GuildId, @IsEnabled)";

                using (SqlConnection myConnection = new SqlConnection(_connectionString))
                {
                    using (SqlCommand oCmd = new SqlCommand(query, myConnection))
                    {
                        oCmd.Parameters.Add("@GuildId", System.Data.SqlDbType.BigInt).Value = guildId;
                        oCmd.Parameters.Add("@IsEnabled", System.Data.SqlDbType.Bit).Value = isEnabled ? 1 : 0;

                        await myConnection.OpenAsync();

                        await oCmd.ExecuteNonQueryAsync();
                    }

                    await myConnection.CloseAsync();
                }
                return true;
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, message: $"Failed to update values for logger status for guild {guildId}.\n Error : {e.Message}");
            }

            return false;
        }

        public async Task<bool> GetLoggerStatusForGuild(ulong guildId)
        {
            string query = "SELECT [is_enabled]FROM [DbDiscord].[dbo].[TbLogger] WHERE [guild_id] = @GuildId";

            bool isLoggerEnabled = false;
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
                                isLoggerEnabled = (bool)oReader["is_enabled"];
                            }

                            myConnection.Close();
                        }
                    }

                    await myConnection.CloseAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, message: $"Failed to retreive values for logger status for guild {guildId}.\n Error : {e.Message}");
            }

            return isLoggerEnabled;
        }
    }
}