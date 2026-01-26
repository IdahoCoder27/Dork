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
            => new GameOutput("I saw you typed something.\n However, the idiot that is designing this game hasn't equipped me with the ability to process that command yet.\nI have filed a formal complaint against the oblivious designer as a result.\n\nCarry on.", OutputKind.Prompt, "UNKNOWN");
    }
}
