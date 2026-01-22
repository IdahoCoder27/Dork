using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class InventoryCommands : ICommandHandler
    {
        public int Priority => 40;

        public bool CanHandle(string input, GameContext ctx)
            => input is "inventory" or "inv" or "i"
               || input.StartsWith("take ") || input.StartsWith("get ")
               || input.StartsWith("drop ") || input.StartsWith("leave ");

        public GameOutput Handle(string input, GameContext ctx)
        {
            if (input is "inventory" or "inv" or "i")
                return Inventory(ctx);

            if (input.StartsWith("take "))
                return Take(ctx, input["take ".Length..].Trim());

            if (input.StartsWith("get "))
                return Take(ctx, input["get ".Length..].Trim());

            if (input.StartsWith("drop "))
                return Drop(ctx, input["drop ".Length..].Trim());

            if (input.StartsWith("leave "))
                return Drop(ctx, input["leave ".Length..].Trim());

            return new GameOutput("Nope.", OutputKind.Prompt);
        }

        private static GameOutput Inventory(GameContext ctx)
        {
            if (ctx.State.Inventory.Count == 0)
                return new GameOutput("You are carrying nothing. Like a minimalist. Against your will.", OutputKind.Narration);

            var lines = ctx.State.Inventory
                .Select(id => ctx.World.GetItem(id).Name)
                .OrderBy(x => x)
                .ToList();

            return new GameOutput("You are carrying:\n- " + string.Join("\n- ", lines), OutputKind.Narration);
        }

        private static GameOutput Take(GameContext ctx, string noun)
        {
            if (string.IsNullOrWhiteSpace(noun))
                return new GameOutput("Take what?", OutputKind.Prompt, "TAKE_WHAT");

            var room = ctx.World.GetRoom(ctx.State.CurrentRoomId);
            var item = ItemResolver.FindInRoomByName(ctx, noun);

            if (item == null)
                return new GameOutput("Not seeing it. Maybe it’s hiding from you.", OutputKind.Prompt, "NO_ITEM");

            if (!item.Capabilities.HasFlag(Model.ItemCapability.Takeable))
                return new GameOutput("You can’t take that.", OutputKind.Prompt, "CANT_TAKE");

            room.ItemIds.Remove(item.Id);
            ctx.State.Inventory.Add(item.Id);

            return new GameOutput($"Taken: {item.Name}.", OutputKind.Narration);
        }

        private static GameOutput Drop(GameContext ctx, string noun)
        {
            if (string.IsNullOrWhiteSpace(noun))
                return new GameOutput("Drop what?", OutputKind.Prompt, "DROP_WHAT");

            var item = ItemResolver.FindInInventoryByName(ctx, noun);

            if (item == null)
                return new GameOutput("You aren’t carrying that.", OutputKind.Prompt, "NOT_CARRYING");

            ctx.State.Inventory.Remove(item.Id);
            ctx.World.GetRoom(ctx.State.CurrentRoomId).ItemIds.Add(item.Id);

            return new GameOutput($"Dropped: {item.Name}.", OutputKind.Narration);
        }
    }
}
