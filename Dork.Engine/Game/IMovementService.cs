using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Game;

namespace Dork.Engine.Game
{
    public interface IMovementService
    {
        GameOutput Go(string direction, GameContext ctx);
        GameOutput MoveTo(int roomId, GameContext ctx, string? reasonCode = null);
    }
}
