using System.Collections.Generic;

namespace Discord_Bot.Services.DataClasses;

public class GuildData
{
    public Dictionary<string, ulong> ChannelsDict = new();
    public Dictionary<string, ulong> RolesDict = new();
    public Dictionary<string, bool> ServiceStatusDict = new();
}