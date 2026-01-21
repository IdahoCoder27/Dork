using Dork.Engine.Commands;
using Dork.Engine.Game;
using Dork.Engine.Model;

namespace Dork.Engine.Commands
{

    public sealed class MovementCommands : ICommandHandler
    {
        public int Priority => 10;

        public bool CanHandle(string input, GameContext ctx)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            // "go north"
            if (input.StartsWith("go ")) return true;

            // Direction-only shorthand (north, n, west...)
            var room = ctx.World.GetRoom(ctx.State.CurrentRoomId);
            return room.Exits.ContainsKey(input);
        }

        public GameOutput Handle(string input, GameContext ctx)
        {
            var dir = input.StartsWith("go ") ? input["go ".Length..].Trim() : input.Trim();
            return ctx.Movement.Go(dir, ctx);
        }
    }
}
