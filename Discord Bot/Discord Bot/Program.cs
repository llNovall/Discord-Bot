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
using DSharpPlus.SlashCommands;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Policy;
using Discord_Bot.SlashCommands;

namespace Discord_Bot
{
    public class Program
    {
        public readonly EventId BotId = new(700, "Tsuki");
        public DiscordClient DiscordClient { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public SlashCommandsExtension SlashCommands { get; private set; }

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
            DiscordClient.SocketClosed += DiscordClient_SocketClosed;
            DiscordClient.Zombied += DiscordClient_Zombied;

            DatabaseManager databaseManager = new(DiscordClient.Logger);
            GuildManager guildManager = new(databaseManager, DiscordClient.Logger);
            Helper helper = new(guildManager);
            SpotifyService spotifyService = new();
            LavalinkMusicService lavalinkMusicService = new(spotifyService, DiscordClient, guildManager);

            var services = new ServiceCollection().AddSingleton<Random>()
                                                              .AddSingleton<LavalinkMusicService>(lavalinkMusicService)
                                                              .AddSingleton<DiscordEmbedBuilderHelper>()
                                                              .AddSingleton<GIFTenorService>()
                                                              .AddSingleton<WhatIsMyMMRService>()
                                                              .AddSingleton<DatabaseManager>(databaseManager)
                                                              .AddSingleton<SpotifyService>(spotifyService)
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

            SlashCommandsConfiguration slashCommandsConfiguration = new()
            {
                Services = services
            };

            SlashCommands = DiscordClient.UseSlashCommands(slashCommandsConfiguration);
            //SlashCommands.RegisterCommands<SModuleTest>();
            SlashCommands.RegisterCommands<SModuleLavalinkMusic>();

            SlashCommands.SlashCommandExecuted += SlashCommands_SlashCommandExecuted;
            SlashCommands.SlashCommandErrored += SlashCommands_SlashCommandErrored;

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

        private Task DiscordClient_Zombied(DiscordClient sender, ZombiedEventArgs e)
        {
            sender.Logger.LogCritical(BotId, $"Failed heartbeat : {e.Failures}");
            e.Handled = true;
            sender.ReconnectAsync();
            return Task.CompletedTask;
        }

        private Task DiscordClient_SocketClosed(DiscordClient sender, SocketCloseEventArgs e)
        {
            sender.Logger.LogCritical(BotId, $"Close Code : {e.CloseCode}, Message : {e.CloseMessage}");
            e.Handled = true;
            sender.ReconnectAsync();
            return Task.CompletedTask;
        }

        private Task DiscordClient_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotId, message: $"Sender : {e.Author.Username},\nChannel : {e.Channel.Name}\nMessage : {e.Message.Content}\nMessage Type : {e.Message.MessageType}");

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
            await Task.Run(() => sender.Client.Logger.LogError(BotId, $"{e.Command} has issues.\nException : {e.Exception.Message}"));
        }

        private async Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            await Task.Run(() => sender.Client.Logger.LogInformation(BotId, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'"));
        }

        private async Task SlashCommands_SlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
        {
            await Task.Run(() => sender.Client.Logger.LogError(BotId, $"{e.Context.QualifiedName} has issues.\nException : {e.Exception.Message}\nStackTrace : {e.Exception.StackTrace}"));
        }

        private async Task SlashCommands_SlashCommandExecuted(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandExecutedEventArgs e)
        {
            await Task.Run(() => sender.Client.Logger.LogInformation(BotId, $"{e.Context.User.Username} successfully executed '{e.Context.QualifiedName}'"));
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