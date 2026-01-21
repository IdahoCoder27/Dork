using Dork.Engine.Game;
using System.Collections.Generic;
using System.Linq;

namespace Dork.Engine.Systems
{
    public sealed class TurnPipeline
    {
        private readonly List<ITurnSystem> _systems;

        public TurnPipeline(IEnumerable<ITurnSystem> systems)
            => _systems = systems.OrderBy(s => s.Order).ToList();

        public GameOutput Run(GameContext ctx, GameOutput output)
        {
            foreach (var sys in _systems)
                output = sys.Apply(ctx, output);

            return output;
        }
    }
}
