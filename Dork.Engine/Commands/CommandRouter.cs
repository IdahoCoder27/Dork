namespace Dork.Engine.Game;

using Dork.Engine.Commands;
using System.Collections.Generic;
using System.Linq;

public sealed class CommandRouter : ICommandRouter
{
    private readonly List<ICommandHandler> _handlers;

    public CommandRouter(IEnumerable<ICommandHandler> handlers)
        => _handlers = handlers.OrderBy(h => h.Priority).ToList();

    public GameOutput Route(string input, GameContext ctx)
    {
        foreach (var h in _handlers)
        {
            if (h.CanHandle(input, ctx))
                return h.Handle(input, ctx);
        }

        return new GameOutput("That accomplishes nothing.", OutputKind.Error, "UNKNOWN");
    }
}

