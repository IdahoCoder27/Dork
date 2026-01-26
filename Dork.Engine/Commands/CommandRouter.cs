namespace Dork.Engine.Game;

using Dork.Engine.Commands;
using Dork.Engine.Model;
using System.Collections.Generic;
using System.Linq;

public sealed class CommandRouter : ICommandRouter
{
    private readonly List<ICommandHandler> _handlers;

    public CommandRouter(IEnumerable<ICommandHandler> handlers)
        => _handlers = handlers.OrderBy(h => h.Priority).ToList();

    public GameOutput Route(string input, GameContext ctx)
    {

        var normalized = (input ?? "").Trim();

        if (ctx.State.IsGameOver)
        {
            if (normalized.ToLowerInvariant() != "new game" && normalized.ToLowerInvariant() != "load game")
                return new GameOutput(ctx.State.GameOverReason ?? "Game over.");
        }

        foreach (var h in _handlers)
        {
            if (h.CanHandle(input, ctx))
                return h.Handle(input, ctx);
        }

        return new GameOutput("That does nothing.", OutputKind.Error, "UNKNOWN");
    }
}

