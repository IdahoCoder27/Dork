using Dork.Engine.Commands;
using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public interface ICommandHandler
    {
        int Priority { get; } // lower runs first
        bool CanHandle(string input, GameContext ctx);
        GameOutput Handle(string input, GameContext ctx);
    }
}
