using Discord_Bot.Commands;
using Discord_Bot.Config;
using Discord_Bot.Database;
using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discord_Bot
{
    public class Program
    {
        public readonly EventId BotId = new(700, "Tsuki");
        public DiscordClient DiscordClient { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        private static void Main()
        {
            var prog = new Program();
            prog.MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            var json = "";
            await using (var fileStream = File.OpenRead("config.json"))
            using (var streamReader = new StreamReader(fileStream, new UTF8Encoding(false)))
                json = await streamReader.ReadToEndAsync();

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            DiscordConfiguration discordConfiguration = new()
            {
                AutoReconnect = true,
                Intents = DiscordIntents.All,
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug
            };

            DiscordClient = new DiscordClient(discordConfiguration);
            DiscordClient.Ready += DiscordClient_Ready;
            DiscordClient.GuildAvailable += Client_GuildAvailable;
            DiscordClient.ClientErrored += Client_ClientError;
            DiscordClient.MessageCreated += DiscordClient_MessageCreated;

            DatabaseManager databaseManager = new(DiscordClient.Logger);
            GuildManager guildManager = new(databaseManager, DiscordClient.Logger);
            Helper helper = new(guildManager);

            var services = new ServiceCollection().AddSingleton<Random>()
                                                              .AddSingleton<LavalinkMusicService>()
                                                              .AddSingleton<DiscordEmbedBuilderHelper>()
                                                              .AddSingleton<GIFTenorService>()
                                                              .AddSingleton<WhatIsMyMMRService>()
                                                              .AddSingleton<DatabaseManager>(databaseManager)
                                                              .AddSingleton<SpotifyService>()
                                                              .AddSingleton<DiscordClient>(DiscordClient)
                                                              .AddSingleton<GuildManager>(guildManager)
                                                              .AddSingleton<Helper>(helper)
                                                              .BuildServiceProvider();

            CommandsNextConfiguration commandsNextConfiguration = new()
            {
                EnableDms = false,
                EnableMentionPrefix = true,
                StringPrefixes = new[] { configJson.Prefix },
                CaseSensitive = false,
                EnableDefaultHelp = true,
                IgnoreExtraArguments = true,
                UseDefaultCommandHandler = true,
                Services = services
            };

            Commands = DiscordClient.UseCommandsNext(commandsNextConfiguration);

            Commands.RegisterCommands<CModuleGreet>();
            Commands.RegisterCommands<CModuleSimpleMaths>();
            Commands.RegisterCommands<CModuleLavalinkMusic>();
            Commands.RegisterCommands<CModuleTeamBuilder>();
            Commands.RegisterCommands<CModuleAdmin>();
            Commands.RegisterCommands<CModuleLeagueOfLegends>();
            Commands.RegisterCommands<CModuleLavalinkPlayer>();
            Commands.RegisterCommands<CModuleLogger>();
            Commands.RegisterCommands<CModuleMessageBuilder>();
            //Commands.RegisterCommands<CModuleTest>();
            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            DiscordClient.UseVoiceNext();

            DiscordClient.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2),
                AckPaginationButtons = true,
                ResponseBehavior = DSharpPlus.Interactivity.Enums.InteractionResponseBehavior.Ack,
                ResponseMessage = "Failed Interaction or something"
            });

            await DiscordClient.ConnectAsync();
            await InitializeLavalink(DiscordClient);

            await Task.Delay(-1);
        }

        private Task DiscordClient_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotId, message: $"Channel : {e.Channel}\nMessage : {e.Message.Content}\nMessage Type : {e.Message.MessageType}");

            return Task.CompletedTask;
        }

        private async Task DiscordClient_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation(BotId, "Client is ready to process events.");

            var guildManager = (GuildManager)Commands.Services.GetService(typeof(GuildManager));
            if (guildManager != null)
                await guildManager.LoadGuildDataFromDatabase(DiscordClient.Guilds.Select(c => c.Value));
        }

        private async Task InitializeLavalink(DiscordClient discordClient)
        {
            //Process lavalinkProcess = new Process();
            //lavalinkProcess.StartInfo.UseShellExecute = false;
            //lavalinkProcess.StartInfo.FileName = "java";
            //lavalinkProcess.StartInfo.Arguments = "-jar D:\\C#Projects\\Tiny Bot\\LavaLink.Lavalink.jar";
            //bool isStarted = lavalinkProcess.Start();

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1",
                Port = 2333
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass",
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            await discordClient.UseLavalink().ConnectAsync(lavalinkConfig);
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            await Task.Run(() => e.Context.Client.Logger.LogError(BotId, $"{e.Command} has issues.\nException : {e.Exception.Message}"));
        }

        private async Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            await Task.Run(() => e.Context.Client.Logger.LogInformation(BotId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'"));
        }

        private async Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            await Task.Run(() => sender.Logger.LogInformation(BotId, $"Guild available: {e.Guild.Name}"));
        }

        private async Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            await Task.Run(() => sender.Logger.LogError(BotId, e.Exception.Message, "Exception occurred"));
        }
    }
}