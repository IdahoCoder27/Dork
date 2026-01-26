using Dork.Engine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
    public static class SaveRules
    {
        public static bool CanSave(GameContext ctx)
        {
            if (ctx.State.IsGameOver)
                return false;

            return ctx.World
                .GetItemsInRoom(ctx.State.CurrentRoomId)
                .Any(i => i.Capabilities.HasFlag(ItemCapability.SavePoint));
        }
    }
}
