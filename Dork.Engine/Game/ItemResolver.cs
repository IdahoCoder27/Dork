using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Model;
using Dork.Engine.World;

namespace Dork.Engine.Game
{
    public static class ItemResolver
    {
        public static Item? FindInRoomByName(GameContext ctx, string token)
        {
            token = InputNormalizer.Normalize(token);
            var room = ctx.World.GetRoom(ctx.State.CurrentRoomId);

            foreach (var id in room.ItemIds)
            {
                var item = ctx.World.GetItem(id);
                if (Matches(item, token))
                    return item;
            }
            return null;
        }

        public static Item? FindInInventoryByName(GameContext ctx, string token)
        {
            token = InputNormalizer.Normalize(token);

            foreach (var id in ctx.State.Inventory)
            {
                var item = ctx.World.GetItem(id);
                if (Matches(item, token))
                    return item;
            }
            return null;
        }

        public static Item? FindInScopeByName(GameContext ctx, string token)
            => FindInInventoryByName(ctx, token) ?? FindInRoomByName(ctx, token);

        private static bool Matches(Item item, string token)
        {
            if (InputNormalizer.Normalize(item.Name) == token)
                return true;

            // optional aliases if you have them
            if (item.Aliases != null)
            {
                foreach (var a in item.Aliases)
                    if (InputNormalizer.Normalize(a) == token)
                        return true;
            }

            return false;
        }
    }
}
