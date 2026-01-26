using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Commands
{
    public sealed class SaveCommands : ICommandHandler
    {
        public int Priority => 10;

        public bool CanHandle(string input, GameContext ctx)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            input = input.Trim().ToLowerInvariant();

            return input == "save"
                || input.StartsWith("save ", StringComparison.Ordinal)
                || input.StartsWith("save game ", StringComparison.Ordinal);
        }

        public GameOutput Handle(string input, GameContext ctx)
        {
            if (!SaveRules.CanSave(ctx))
            {
                return new GameOutput(
                    "You have nowhere appropriate to file this situation."
                );
            }

            var save = SaveGameBuilder.From(ctx);
            ctx.SaveService.Write(save);

            return new GameOutput(
                "The current state is filed.\n" +
                "Any previous record is now obsolete."
            );
        }
    }
}
