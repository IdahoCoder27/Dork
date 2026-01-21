using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Model;

namespace Dork.Engine.Game
{
    public sealed class MovementService : IMovementService
    {
        public GameOutput Go(string direction, GameContext ctx)
        {
            var dir = InputNormalizer.Normalize(direction);

            var state = ctx.State;
            var world = ctx.World;

            var fromRoom = world.GetRoom(state.CurrentRoomId);
            var wasDarkAndUnlit = fromRoom.IsDark && !state.PhoneLightOn;

            if (!fromRoom.Exits.TryGetValue(dir, out var exit))
                return new GameOutput("You can’t go that way.", OutputKind.Error, "NO_EXIT");

            if (!exit.IsAllowed(state))
                return new GameOutput(exit.LockedMessage ?? "Access denied.", OutputKind.Error, "EXIT_BLOCKED");

            state.MoveTo(exit.ToRoomId);

            // moving breaks hiding
            state.ClearFlag("player_hidden");

            // mark turn fact for systems
            ctx.Turn.PlayerMoved = true;
            if (wasDarkAndUnlit)
                ctx.Turn.MadeNoise = true;

            return RoomDescriber.Look(ctx);
        }

        public GameOutput MoveTo(int roomId, GameContext ctx, string? reasonCode = null)
        {
            ctx.State.MoveTo(roomId);
            ctx.State.ClearFlag("player_hidden");
            ctx.Turn.PlayerMoved = true;

            return RoomDescriber.Look(ctx);
        }
    }
}
