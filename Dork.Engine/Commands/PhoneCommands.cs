using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class PhoneCommands : ICommandHandler
    {
        public int Priority => 100;

        public bool CanHandle(string input, GameContext ctx)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Input is already normalized by DorkGame.Execute via InputNormalizer.Normalize(...)
            // Keep matching intentionally narrow so we don't steal other "read" commands.
            return input == "messages"
                || input == "message"
                || input == "texts"
                || input == "text"
                || input == "check messages"
                || input == "check phone"
                || input == "open phone"
                || input == "read messages"
                || input == "read message"
                || input == "read texts"
                || input == "read text"
                || input == "read phone";
        }

        public GameOutput Handle(string input, GameContext ctx)
        {
            // Must have the phone (item id 1 in WorldFactory)
            // Using both id and resolver keeps this resilient if ids shift later.
            var hasPhone = ctx.State.HasItem(1) || ItemResolver.FindInInventoryByName(ctx, "phone") != null;

            if (!hasPhone)
                return new GameOutput("You don't have a phone.", OutputKind.Error, "NO_PHONE");

            if (ctx.State.PhoneBattery <= 0)
                return new GameOutput("The phone is dead. It offers no wisdom.", OutputKind.Error, "PHONE_DEAD");

            if (ctx.State.PhoneUnreadMessages <= 0)
                return new GameOutput("No unread messages.", OutputKind.Narration, "NO_UNREAD");

            ctx.State.MarkPhoneMessageRead();

            // The message text already includes the game's tone. Don't over-format it here.
            return new GameOutput(ctx.State.PhoneMessageText, OutputKind.Narration, "PHONE_MSG");
        }
    }
}
