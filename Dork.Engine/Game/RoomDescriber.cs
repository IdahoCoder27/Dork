using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
    public static class RoomDescriber
    {
        public static GameOutput Look(GameContext ctx)
        {
            var room = ctx.World.GetRoom(ctx.State.CurrentRoomId);
            var sb = new StringBuilder();

            // Title (optional, but nice)
            if (!string.IsNullOrWhiteSpace(room.Title))
            {
                sb.AppendLine(room.Title);
                sb.AppendLine();
            }

            // Room description
            sb.AppendLine(room.Description);

            // Items in room (uses ItemIds)
            var itemIds = room.ItemIds?.Where(id => id < 999).ToList() ?? new List<int>();
            if (itemIds.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("You see:");

                foreach (var itemId in itemIds)
                {
                    var item = ctx.World.GetItem(itemId);
                    sb.AppendLine($"- {item.Name}");
                }
            }

            // Exits (direction is the dictionary key)
            var exits = room.Exits
                .Where(kvp => !kvp.Value.IsHidden) // hide hidden exits
                .ToList();

            if (exits.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Exits:");

                foreach (var (dir, exit) in exits)
                {
                    // If you want to hide locked exits completely, filter with exit.IsAllowed(ctx.State)
                    sb.AppendLine($"- {dir}");
                }
            }

            return new GameOutput(sb.ToString(), OutputKind.Narration, null);
        }
    }
}
