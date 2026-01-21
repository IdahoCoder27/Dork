namespace Dork.Engine.Game;

using Dork.Engine.Commands;

public interface ICommandRouter
{
    GameOutput Route(string input, GameContext ctx);
}
