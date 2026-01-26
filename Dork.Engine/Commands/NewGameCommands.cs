using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class NewGameCommands : ICommandHandler
    {
        public int Priority => -100;

        public bool CanHandle(string input, GameContext ctx)
            => input is "new game" or "new";

        public GameOutput Handle(string input, GameContext ctx)
        {
            ctx.State.NewGameRequested = true;
            return new GameOutput("A new case file has been opened. Previous work is no longer relevant.\n");
        }
    }
}
