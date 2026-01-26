using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class WaitCommands : ICommandHandler
    {
        public int Priority => 10;

        public bool CanHandle(string input, GameContext ctx)
        {
            return input is "wait" or "z" or "pause";
        }

        public GameOutput Handle(string input, GameContext ctx)
        {
            return new GameOutput("You wait. Nothing happens.");
        }
    }
}
