using Discord_Bot.Database;
using Discord_Bot.Services;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tiny_Bot.Commands;
using Tiny_Bot.Services;

namespace Tiny_Bot
{
    public class Program
    {
        public readonly EventId BotID = new EventId(700, "Tsuki");
        public DiscordClient DiscordClient { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        private static void Main(string[] args)
        {
            var prog = new Program();
            prog.MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            string json = "";
            using (FileStream fileStream = File.OpenRead("config.json"))
            using (StreamReader streamReader = new StreamReader(fileStream, new UTF8Encoding(false)))
                json = await streamReader.ReadToEndAsync();

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            DiscordConfiguration discordConfiguration = new DiscordConfiguration()
            {
                AutoReconnect = true,
                Intents = DiscordIntents.AllUnprivileged,
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Debug
            };

            DiscordClient = new DiscordClient(discordConfiguration);
            DiscordClient.Ready += DiscordClient_Ready;
            DiscordClient.GuildAvailable += Client_GuildAvailable;
            DiscordClient.ClientErrored += Client_ClientError;
            DiscordClient.MessageCreated += DiscordClient_MessageCreated;

            ServiceProvider services = new ServiceCollection().AddSingleton<Random>()
                                                              .AddSingleton<LavalinkMusicService>()
                                                              .AddSingleton<DiscordEmbedBuilderHelper>()
                                                              .AddSingleton<GIFTenorService>()
                                                              .AddSingleton<WhatIsMyMMRService>()
                                                              .AddSingleton<DatabaseManager>()
                                                              .AddSingleton<ChannelFinder>()
                                                              .BuildServiceProvider();

            CommandsNextConfiguration commandsNextConfiguration = new CommandsNextConfiguration()
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

            Commands.CommandExecuted += Commands_CommandExecuted;
            Commands.CommandErrored += Commands_CommandErrored;

            DiscordClient.UseVoiceNext();

            await DiscordClient.ConnectAsync();
            await InitializeLavalink(DiscordClient);

            await Task.Delay(-1);
        }

        private Task DiscordClient_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotID, $"Channel : {e.Channel}\nMessage : {e.Message.Content}\nMessage Type : {e.Message.MessageType}");

            return Task.CompletedTask;
        }

        private Task DiscordClient_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            sender.Logger.LogInformation(BotID, "Client is ready to process events.");
            return Task.CompletedTask;
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
            await Task.Run(() => e.Context.Client.Logger.LogError(BotID, $"{e.Command} has issues.\nException : {e.Exception.Message}"));
        }

        private async Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            await Task.Run(() => e.Context.Client.Logger.LogInformation(BotID, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'"));
        }

        private async Task Client_GuildAvailable(DiscordClient sender, GuildCreateEventArgs e)
        {
            await Task.Run(() => sender.Logger.LogInformation(BotID, $"Guild available: {e.Guild.Name}"));
        }

        private async Task Client_ClientError(DiscordClient sender, ClientErrorEventArgs e)
        {
            await Task.Run(() => sender.Logger.LogError(BotID, e.Exception.Message, "Exception occured"));
        }
    }
}