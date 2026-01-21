using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class ExamineCommands : ICommandHandler
    {
        public int Priority => 10;

        public bool CanHandle(string input, GameContext ctx)
            => input is "look" or "l";

        public GameOutput Handle(string input, GameContext ctx)
        => RoomDescriber.Look(ctx);
    }
}
