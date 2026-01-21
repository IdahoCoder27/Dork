using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class ExamineCommands : ICommandHandler
    {
        public int Priority => 20;

        public bool CanHandle(string input, GameContext ctx)
            => input is "look" or "l" || input.StartsWith("examine ") || input.StartsWith("x ");

        public GameOutput Handle(string input, GameContext ctx)
        {
            if (input is "look" or "l")
                return RoomDescriber.Look(ctx);

            var noun = input.StartsWith("examine ") ? input["examine ".Length..].Trim()
                     : input.StartsWith("x ") ? input["x ".Length..].Trim()
                     : "";

            if (string.IsNullOrWhiteSpace(noun))
                return new GameOutput("Examine what, exactly?", OutputKind.Prompt, "EXAMINE_WHAT");

            var item = ItemResolver.FindInScopeByName(ctx, noun);
            if (item == null)
                return new GameOutput("You don’t see that here.", OutputKind.Prompt, "NO_SUCH_THING");

            var baseText = string.IsNullOrWhiteSpace(item.Description)
                ? $"It’s {item.Name}. It exists. Congratulations."
                : item.Description;

            // Default: just the description.
            // Phone special-case: only show battery + unread notification if it's in hand (inventory).
            if (item.Phone is null)
                return new GameOutput(baseText, OutputKind.Narration);

            var inHand = ctx.State.HasItem(item.Id); // "in my hand" == in inventory in your model
            if (!inHand)
                return new GameOutput(baseText, OutputKind.Narration);

            var battery = ctx.State.PhoneBattery;
            var lightOn = ctx.State.PhoneLightOn;

            var batteryDesc = battery switch
            {
                >= 80 => "Battery level: healthy. For now.",
                >= 50 => "Battery level: acceptable, but the future is approaching.",
                >= 25 => "Battery level: concerning.",
                >= 10 => "Battery level: blinking icon territory.",
                > 0 => "Battery level: critically low. You knew this was coming.",
                _ => "Battery level: dead. Emotionally and electrically."
            };

            var sb = new StringBuilder();
            sb.AppendLine(baseText);
            sb.AppendLine();
            sb.AppendLine(batteryDesc);
            if (lightOn)
            {
                sb.AppendLine("The flashlight is currently on, burning precious electrons.");
            }


            if (ctx.State.PhoneUnreadMessages > 0)
            {
                sb.AppendLine("You have 1 unread message. It doesn't look urgent. That's worse.");
            }

            return new GameOutput(sb.ToString().TrimEnd(), OutputKind.Narration);
        }
    }
}
