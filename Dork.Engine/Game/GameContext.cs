using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.World;
using Dork.Engine.Model;

namespace Dork.Engine.Game
{
    public sealed class GameContext
    {
        public World.World World { get; }
        public GameState State { get; }
        public GameOptions Options { get; }
        public Random Rng { get; }
        public IMovementService Movement { get; }

        // Per-turn scratchpad (NOT persisted)
        public TurnFrame Turn { get; } = new();

        public GameContext(World.World world, GameState state, GameOptions options, Random rng, MovementService movement)
        {
            World = world;
            State = state;
            Options = options;
            Rng = rng;
            Movement = movement;
        }
    }

    public sealed class TurnFrame
    {
        public string RawInput { get; set; } = "";
        public string NormalizedInput { get; set; } = "";
        public bool PlayerMoved { get; set; }
        public bool MadeNoise { get; set; }
        public bool PresentedId { get; set; }
    }
}
