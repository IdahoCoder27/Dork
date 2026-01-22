using Dork.Engine.Game;
using System;

namespace Dork.Engine.Commands
{
    public sealed class PushCommands : ICommandHandler
    {
        // Needs to run before UNKNOWN, but after your more specific stuff.
        // Inventory is 40. Phone/Security you can leave where they are.
        public int Priority => 55;

        public bool CanHandle(string input, GameContext ctx)
            => input.StartsWith("push ") || input.StartsWith("press ");

        public GameOutput Handle(string input, GameContext ctx)
        {
            var verb = input.StartsWith("push ") ? "push " : "press ";
            var noun = input[verb.Length..].Trim();

            if (string.IsNullOrWhiteSpace(noun))
                return new GameOutput($"{Cap(verb.Trim())} what?", OutputKind.Prompt, "PUSH_WHAT");

            // If they press/push an EXIT label, treat it as movement.
            var room = ctx.World.GetRoom(ctx.State.CurrentRoomId);

            if (room.Exits.ContainsKey(noun))
            {
                // Do NOT bypass rules: route through the same movement logic
                // your game already uses (MovementService).
                return ctx.Movement.Go(noun, ctx);
            }

            // Not an exit: later this can become "push item" interaction.
            return new GameOutput("You push at it. Reality remains unchanged.", OutputKind.Narration, "PUSH_NO_EFFECT");
        }

        private static string Cap(string s)
            => string.IsNullOrEmpty(s) ? s : char.ToUpperInvariant(s[0]) + s[1..];
    }
}

