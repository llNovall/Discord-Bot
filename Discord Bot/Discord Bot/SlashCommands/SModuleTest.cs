using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discord_Bot.SlashCommands
{
    public class SModuleTest : ApplicationCommandModule
    {
        [SlashCommand("test", "A slash command made to test the DSharpPlus Slash Commands extension!")]
        public async Task TestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Success!"));
        }

        [SlashCommand("delaytest", "A slash command made to test the DSharpPlus Slash Commands extension!")]
        public async Task DelayTestCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            //Some time consuming task like a database call or a complex operation

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Thanks for waiting!"));
        }

        public class TestChoiceProvider : IChoiceProvider
        {
            public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider()
            {
                return new DiscordApplicationCommandOptionChoice[]
                {
                    //You would normally use a database call here
                    new DiscordApplicationCommandOptionChoice("testing", "testing"),
                    new DiscordApplicationCommandOptionChoice("testing2", "test option 2")
                };
            }
        }

        [SlashCommand("choiceprovider", "test")]
        public async Task ChoiceProviderCommand(InteractionContext ctx,
            [ChoiceProvider(typeof(TestChoiceProvider))]
            [Option("option", "option")] string option)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(option));
        }
    }
}