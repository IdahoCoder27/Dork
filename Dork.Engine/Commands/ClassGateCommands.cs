using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Game;
using Dork.Engine.Model;

namespace Dork.Engine.Commands
{
    public sealed class ClassGateCommands : ICommandHandler
    {
        public int Priority => 0; // must run before everything

        public bool CanHandle(string input, GameContext ctx)
        {
            // Only handle when class not chosen yet
            return ctx.State.Class == PlayerClass.None;
        }

        public GameOutput Handle(string input, GameContext ctx)
        {
            var welcome = "";
            if (!ctx.State.HasFlag("welcome_shown"))
            {
                ctx.State.SetFlag("welcome_shown");
                welcome =
                    "Welcome to DORK.\n" +
                    "A text adventure that judges your input.\n\n" +
                    "Your goal is simple: A leak has been discovered at a clandestine organization.\nFind the leak.\n\n";
            }

            var lower = input.Trim().ToLowerInvariant();
            lower = lower.Trim().TrimEnd('\\', '/', '.', '!');

            if (lower is "classes" or "class")
                return ShowClasses(ctx);

            // Accept: "choose janitor" / "choose manager" / "choose intern" etc.
            if (lower.StartsWith("choose "))
            {
                var pick = lower["choose ".Length..].Trim();
                return Choose(pick, ctx);
            }

            // Allow bare class names too (janitor/manager/intern)
            if (lower is "janitor" or "manager" or "intern")
                return Choose(lower, ctx);

            // First time prompt, or any other input before choosing
            return new GameOutput("Type `classes` to see your limited options, protagonist.", OutputKind.Prompt, "CLASS_GATE");
        }

        private static GameOutput ShowClasses(GameContext ctx)
            => new GameOutput(
                "Pick a class. Because apparently you need labels to fail properly.\n\n" +
                "Available:\n" +
                "- janitor\n" +
                "- intern\n" +
                "- manager\n" +
                "- necromancer\n\n" +
                "Type: choose <class> or <class>\n" +
                "Example: choose intern",
                OutputKind.Prompt,
                "CLASSES"
            );

        private static GameOutput Choose(string pick, GameContext ctx)
        {
            var cls = pick switch
            {
                "janitor" => PlayerClass.Janitor,
                "middle manager" or "middlemanager" or "manager" => PlayerClass.MiddleManager,
                "intern" => PlayerClass.Intern,
                "necromancer" => PlayerClass.Necromancer,
                _ => PlayerClass.None
            };

            if (cls == PlayerClass.None)
                return new GameOutput("That class does not exist. Shocking.", OutputKind.Error, "BAD_CLASS");

            ctx.State.Class = cls;

            var intro = cls switch
            {
                PlayerClass.Janitor =>
                    "Janitor selected.\n" +
                    "You wield keys, patience, and the quiet rage of someone who has seen things no one thanked you for.",

                PlayerClass.Intern =>
                    "Intern selected.\n" +
                    "You possess limitless optimism, zero permissions, and the dangerous belief that this might lead somewhere.",

                PlayerClass.MiddleManager =>
                    "Middle Manager selected.\n" +
                    "Your special ability is scheduling meetings that kill momentum while producing no evidence of blame.",

                PlayerClass.Necromancer =>
                    "Necromancer selected.\n" +
                    "Finally, someone qualified to debug legacy systems.\n" +
                    "HR will pretend not to notice.",

                _ =>
                    "Class selected.\n" +
                    "Somehow, this feels like a mistake."
            };

            // After choosing, show room description
            var look = RoomDescriber.Look(ctx);

            return new GameOutput(intro + "\n\n" + look.Text, OutputKind.Narration, "LOOK");
        }
    }
}
