using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Game;

namespace Dork.Engine.Commands
{
    public sealed class FallbackCommands : ICommandHandler
    {
        public int Priority => 9999;

        public bool CanHandle(string input, GameContext ctx) => true;

        public GameOutput Handle(string input, GameContext ctx)
            => new GameOutput("Unrecognized command. Type `help` if you want to be told the obvious.", OutputKind.Prompt, "UNKNOWN");
    }
}
