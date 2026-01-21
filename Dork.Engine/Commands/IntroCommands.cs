using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class IntroCommands : ICommandHandler
    {
        public int Priority => -1000; // Always first

        public bool CanHandle(string input, GameContext ctx)
        {
            // Only run once per game session
            return !ctx.State.HasFlag("intro_shown");
        }

        public GameOutput Handle(string input, GameContext ctx)
        {
            ctx.State.SetFlag("intro_shown");

            return new GameOutput(
                "Welcome to DORK.\n" +
                "DORK is a counter-intelligence game.\n\n" +
                "There is an internal leak.\n" +
                "Someone is selling secrets.\n\n" +
                "Your job is to find them and stop it.\n\n" +
                "The systems you work inside are rigid.\n" +
                "They do not care what you meant.\n\n" +
                "They will kill you for behaving like you're in a game.\n" +
                "Try not to touch anything important.\n" +
                "Or do. It’ll be fun to watch.\n\n" +
                "Type `classes` to select the title you’ll blame this on later.",
                OutputKind.Narration,
                "INTRO"
            );
        }
    }
}
