using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using System;
using System.Threading.Tasks;

namespace Discord_Bot.Commands
{
    internal class CModuleTest : BaseCommandModule
    {
        public Random Rng { private get; set; }

        //[Command("greet")]
        //public async Task GreetCommand(CommandContext ctx, [RemainingText] string name)
        //{
        //    await ctx.RespondAsync($"Greetings, {string.Join(" ", name)}!");
        //}

        //[Command("greet")]
        //public async Task GreetCommand(CommandContext ctx, DiscordMember discordMember)
        //{
        //    await ctx.Channel.SendMessageAsync($"Greetings, {string.Join(" ", discordMember.Mention)}!");
        //}

        [Command("random")]
        public async Task RandomCommand(CommandContext ctx, int min, int max)
        {
            await ctx.RespondAsync($"Your number is {Rng.Next(min, max)}");
        }

        [Command("testbutton")]
        public async Task TestButton(CommandContext ctx)
        {
            var builder = new DiscordMessageBuilder()
            .WithContent("This message has buttons! Pretty neat innit?")
            .AddComponents(new DiscordComponent[]
            {
                new DiscordButtonComponent(ButtonStyle.Primary, "1_top", "Blurple!"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "2_top", "Grey!"),
                new DiscordButtonComponent(ButtonStyle.Success, "3_top", "Green!"),
                new DiscordButtonComponent(ButtonStyle.Danger, "4_top", "Red!"),
                new DiscordLinkButtonComponent("https://some-super-cool.site", "Link!")
            });

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                await e.Interaction.CreateResponseAsync(
        InteractionResponseType.UpdateMessage,
        new DiscordInteractionResponseBuilder()
            .WithContent("No more buttons for you >:)"));
            };

            await ctx.RespondAsync(builder);
        }

        private DiscordMessage _searchMessage;
        private DiscordMessage _playlistMessage;

        [Command("search")]
        public async Task SearchUri(CommandContext ctx)
        {
            ctx.Client.ModalSubmitted += MusicPlayerModalSubmitted;
            ctx.Client.ComponentInteractionCreated += MusicPlayerComponentInteractionCreated;

            string prepareContent = "Preparing Channel for Music";

            DiscordMessage prepareMessage = await ctx.Channel.SendMessageAsync(prepareContent);

            var response = new DiscordInteractionResponseBuilder();

            response
              .WithTitle("Super cool modal!")
              .WithCustomId("my-modal")
              .AddComponents(new TextInputComponent(label: "Favorite food", customId: "fav-food", placeholder: "Pizza, Icecream, etc", max_length: 30))
              .AddComponents(new TextInputComponent("Why?", "why-fav", "Because it tastes good", required: false, style: TextInputStyle.Paragraph));

            var search = new DiscordMessageBuilder()
                .WithContent("Search : ")
                .AddComponents(
                    new DiscordComponent[] {
                        new DiscordButtonComponent (style: ButtonStyle.Secondary, customId: "search", label: "Search", disabled: false)
                    }
                );

            _searchMessage = await ctx.Channel.SendMessageAsync(search);

            prepareContent += "\n + Added search box to search for tracks.";
            await prepareMessage.ModifyAsync(prepareContent);

            string playlistContent = GetPlaylistFrom(1, 10);

            var player = CreatePlayer(playlistContent, ctx.Client);

            _playlistMessage = await ctx.Channel.SendMessageAsync(player);
        }

        private Task MusicPlayerComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            throw new NotImplementedException();
        }

        private Task MusicPlayerModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async Task SearchInteraction(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            var response = new DiscordInteractionResponseBuilder();
            response
             .WithTitle("Super cool modal!")
             .WithCustomId("my-modal")
             .AddComponents(new TextInputComponent(label: "Favorite food", customId: "fav-food", placeholder: "Pizza, Icecream, etc", max_length: 30))
             .AddComponents(new TextInputComponent("Why?", "why-fav", "Because it tastes good", required: false, style: TextInputStyle.Paragraph));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, response);
        }

        [Command("poll")]
        public async Task PollCommand(CommandContext ctx)
        {
            string copy = "A really long name of something *and something and something* and something";

            string contentString = "";

            //for (int i = 0; i < num; i++)
            //{
            //    contentString += $"{i + 1}) {copy}\n";
            //}

            //DiscordButtonComponent left = new DiscordButtonComponent(ButtonStyle.Secondary, "music_left", "Left", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:")));
            //DiscordButtonComponent right = new DiscordButtonComponent(ButtonStyle.Secondary, "music_right", "Right", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:")));
            //DiscordButtonComponent play = new DiscordButtonComponent(ButtonStyle.Secondary, "music_play", "Play", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":play_pause:")));
            //DiscordButtonComponent stop = new DiscordButtonComponent(ButtonStyle.Secondary, "music_stop", "Stop", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":stop_button:")));

            //var messageBuilder = new DiscordMessageBuilder()
            //    .WithContent(contentString)
            //    .AddComponents(new DiscordComponent[]
            //    {
            //       left, right, play, stop

            //    });

            contentString = GetPlaylistFrom(1, 10);
            DiscordMessageBuilder messageBuilder = CreatePlayer(contentString, ctx.Client);

            ctx.Client.ComponentInteractionCreated += SearchInteraction;
            var message = await ctx.RespondAsync(messageBuilder);
        }

        private async Task Client_ComponentInteractionCreated(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
            string contentString = GetPlaylistFrom(11, 20);
            DiscordMessageBuilder messageBuilder = CreatePlayer(contentString, sender);

            //string content = $"Name : {e.Interaction.Data.Name},\n Id : {e.Interaction.Data.Id},\n Values : {e.Interaction.Data.Values},\n Title : {e.Interaction.Data.Title},\n Components : {e.Interaction.Data.Components},\n ComponentType : {e.Interaction.Data.ComponentType},\n CustomId : {e.Interaction.Data.CustomId}";
            DiscordInteractionResponseBuilder responseBuilder = new DiscordInteractionResponseBuilder(messageBuilder);
            //await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, responseBuilder);

            var response = new DiscordInteractionResponseBuilder();

            response
              .WithTitle("Super cool modal!")
              .WithCustomId("my-modal")
              .AddComponents(new TextInputComponent(label: "Favorite food", customId: "fav-food", placeholder: "Pizza, Icecream, etc", max_length: 30))
              .AddComponents(new TextInputComponent("Why?", "why-fav", "Because it tastes good", required: false, style: TextInputStyle.Paragraph));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, responseBuilder);
        }

        private string GetPlaylistFrom(int start, int end)
        {
            string copy = "A really long name of something *and something and something* and something";
            string contentString = "";

            for (int i = start; i < end; i++)
            {
                contentString += $"{i + 1}) {copy}\n";
            }

            return contentString;
        }

        private DiscordMessageBuilder CreatePlayer(string contentString, DiscordClient discordClient)
        {
            DiscordButtonComponent left = new DiscordButtonComponent(ButtonStyle.Secondary, "music_left", "Left", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":arrow_left:")));
            DiscordButtonComponent right = new DiscordButtonComponent(ButtonStyle.Secondary, "music_right", "Right", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":arrow_right:")));
            DiscordButtonComponent play = new DiscordButtonComponent(ButtonStyle.Secondary, "music_play", "Play", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":play_pause:")));
            DiscordButtonComponent stop = new DiscordButtonComponent(ButtonStyle.Secondary, "music_stop", "Stop", emoji: new DiscordComponentEmoji(DiscordEmoji.FromName(discordClient, ":stop_button:")));

            var messageBuilder = new DiscordMessageBuilder()
                .WithContent(contentString)
                .AddComponents(new DiscordComponent[]
                {
                   left, right, play, stop
                });

            return messageBuilder;
        }
    }
}