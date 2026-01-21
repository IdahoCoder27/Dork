using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Game;

namespace Dork.Engine.Commands
{
    public sealed class HelpCommands : ICommandHandler
    {
        public int Priority => 30;

        public bool CanHandle(string input, GameContext ctx)
            => input is "help" or "?";

        public GameOutput Handle(string input, GameContext ctx)
            => new GameOutput(
                "Commands:\n" +
                "- look (l)\n" +
                "- go <direction>\n" +
                "- inventory (i)\n" +
                "- take/get <item>\n" +
                "- drop/leave <item>\n" +
                "- examine/x <thing>\n\n" +
                "Try not to improvise too hard yet. I’m still recovering.",
                OutputKind.Narration
            );
    }
}
