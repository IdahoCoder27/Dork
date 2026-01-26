using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
    public static class SaveGameBuilder
    {
        public static SaveGame From(GameContext ctx)
        {
            return new SaveGame
            {
                CurrentRoomId = ctx.State.CurrentRoomId,
                InventoryItemIds = ctx.State.Inventory.ToList(),
                Flags = new HashSet<string>(ctx.State.Flags)
            };
        }
    }
}
