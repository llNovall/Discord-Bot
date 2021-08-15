using DSharpPlus;
using DSharpPlus.CommandsNext;
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
using Tiny_Bot;
using Tiny_Bot.Commands;
using Tiny_Bot.DataClasses;
using Tiny_Bot.Utility;

namespace Discord_Bot
{
    class Program
    {
        private static EventId _botID = new EventId(700, "Tsuki");

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        internal static async Task MainAsync()
        {
            string json = "";
            using (FileStream fileStream = File.OpenRead("config.json"))
            using (StreamReader streamReader = new StreamReader(fileStream, new UTF8Encoding(false)))
                json = await streamReader.ReadToEndAsync();

            ConfigJson configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            DiscordClient discordClient = new DiscordClient(new DiscordConfiguration()
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            });

            //CModuleLavalinkMusicData data = new CModuleLavalinkMusicData();

            ServiceProvider services = new ServiceCollection().AddSingleton<Random>()
                                                              .AddSingleton<CModuleLavalinkMusicData>()
                                                              .AddSingleton<DiscordEmbedBuilderHelper>()
                                                              .AddSingleton<GIFTenorService>()
                                                              .BuildServiceProvider();

            CommandsNextExtension commands = discordClient.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" },
                Services = services
            });

            commands.RegisterCommands<CModuleGreet>();
            commands.RegisterCommands<CModuleSimpleMaths>();
            commands.RegisterCommands<CModuleLavalinkMusic>();
            commands.RegisterCommands<CModuleTeamBuilder>();
            commands.RegisterCommands<CModuleAdmin>();

            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;

            discordClient.UseVoiceNext();

            await discordClient.ConnectAsync();
            await InitializeLavalink(discordClient);

            await Task.Delay(-1);
        }

        private async static Task InitializeLavalink(DiscordClient discordClient)
        {
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
        private async static Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            await e.Context.RespondAsync(e.Exception.Message);
        }

        private static Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(_botID, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");

            return Task.CompletedTask;
        }
    }
}
