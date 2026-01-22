using Dork.Engine.Game;
using System;
using System.Linq;
using System.Text;

namespace Dork.Engine.Commands
{
    public sealed class SecurityCommands : ICommandHandler
    {
        // lower runs first (per ICommandHandler comment) :contentReference[oaicite:3]{index=3}
        public int Priority => 60;

        private const int ElevatorRoomId = 1;
        private const string UnlockFlag = "elevator_ssb_unlocked";
        private const string FailCounter = "elevator_voice_fail";

        public bool CanHandle(string input, GameContext ctx)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;

            // Input is already normalized by the game pipeline; still keep this strict.
            return input == "say"
                || input.StartsWith("say ")
                || input == "speak"
                || input.StartsWith("speak ");
        }

        public GameOutput Handle(string input, GameContext ctx)
        {
            // Outside the elevator, "say" is just you talking to yourself.
            if (ctx.State.CurrentRoomId != ElevatorRoomId)
                return new GameOutput("You speak. The universe refuses to acknowledge it.", OutputKind.Narration, "NO_LISTENER");

            // Extract phrase
            var phrase =
                input == "say" || input == "speak"
                    ? ""
                    : input.StartsWith("say ")
                        ? input["say ".Length..].Trim()
                        : input["speak ".Length..].Trim();

            if (string.IsNullOrWhiteSpace(phrase))
                return new GameOutput("Say what, exactly?", OutputKind.Prompt, "SAY_WHAT");

            // Sanitize in case InputNormalizer doesn't remove punctuation
            var cleaned = StripPunctuationToSpaces(phrase).ToLowerInvariant();
            var tokens = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // Accept both "it clearly" and "say it clearly" patterns.
            // This does NOT give away the solution in failure paths.
            if (IsItClearly(tokens))
            {
                ctx.State.SetFlag(UnlockFlag);
                // Optional: reset fails when they succeed
                // ctx.State.Counters.Remove(FailCounter);

                return new GameOutput("A tiny indicator beside ‘SSB’ flickers once.\r\nThe elevator returns to ignoring you.", OutputKind.Narration, "SSB_UNLOCKED");
            }

            // Rigid + snarky, but no hints
            var n = ctx.State.IncrementCounter(FailCounter);

            var text = n switch
            {
                1 => "ACCESS DENIED.",
                2 => "Still no.",
                3 => "Guessing continues to be a bold strategy.",
                _ => "Aww. Did we break you on the first puzzle already?"
            };

            return new GameOutput(text, OutputKind.Narration, "VOICE_REJECTED");
        }

        private static bool IsItClearly(string[] tokens)
        {
            // exact adjacent match is cleanest
            for (int i = 0; i < tokens.Length - 1; i++)
            {
                if (tokens[i] == "it" && tokens[i + 1] == "clearly")
                    return true;
            }

            // fallback: allow "it" and "clearly" anywhere, in case player says extra fluff
            return tokens.Contains("it") && tokens.Contains("clearly");
        }

        private static string StripPunctuationToSpaces(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
            {
                sb.Append(char.IsLetterOrDigit(ch) ? ch : ' ');
            }
            return sb.ToString();
        }
    }
}
