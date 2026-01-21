using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Systems
{
    public sealed class BatterySystem : ITurnSystem
    {
        public int Order => 100;

        public GameOutput Apply(GameContext ctx, GameOutput current)
        {
            // Battery logic not implemented yet.
            // Do nothing.
            return current;
        }
    }
}
