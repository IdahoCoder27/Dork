using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Game;

namespace Dork.Engine.Systems
{
    public interface ITurnSystem
    {
        int Order { get; } // lower runs first
        GameOutput Apply(GameContext ctx, GameOutput current);
    }
}
