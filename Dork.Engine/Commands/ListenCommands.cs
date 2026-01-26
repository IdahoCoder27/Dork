using Dork.Engine.Game;
using Dork.Engine.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class ListenCommands : ICommandHandler
    {
        public int Priority => 10;

        public bool CanHandle(string input, GameContext ctx)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim().ToLowerInvariant();

            // support: listen | listen to X | listen X
            return input == "listen"
                || input.StartsWith("listen ", StringComparison.Ordinal)
                || input.StartsWith("listen to ", StringComparison.Ordinal);
        }
        public GameOutput Handle(string input, GameContext ctx)
        {
            var room = ctx.World.GetRoom(ctx.State.CurrentRoomId);

            if (ctx.State.TurnsUntilGuard <= 2 && ctx.State.TurnsUntilGuard > 0)
                return new GameOutput(RandomApproachingGuardLine(), OutputKind.Prompt);

            // 2) Room-authored listen text
            if (!string.IsNullOrWhiteSpace(room.ListenText))
                return new GameOutput(room.ListenText, OutputKind.Narration);

            // 3) Fallback ambience / silence
            return new GameOutput(RandomAmbientLine(), OutputKind.Narration);
        }

        private static string RandomAmbientLine()
        {
            string[] lines =
            {
                "You listen. HVAC whispers corporate secrets into the ceiling tiles.",
                "Fluorescent lights hum with the confidence of a company that can’t be sued properly.",
                "Distant ventilation. Somewhere, a printer suffers.",
                "Silence. The kind that suggests someone is paid to keep it that way."
            };

            return lines[Random.Shared.Next(lines.Length)];
        }

        private static string RandomApproachingGuardLine()
        {
            string[] lines =
            {
                "Footsteps. Controlled. Close enough that you stop pretending you’re safe.",
                "A faint scuff of shoes. Someone is walking like they belong here.",
                "Keys clink once. Not nearby. Not far."
            };

            return lines[Random.Shared.Next(lines.Length)];
        }
    }
}
