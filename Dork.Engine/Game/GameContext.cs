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
        public MovementService Movement { get; }
        public ISaveService SaveService { get; }

        public TurnFrame Turn { get; } = new();

        public GameContext(
            World.World world,
            GameState state,
            GameOptions options,
            Random rng,
            MovementService movement,
            ISaveService saveService)
        {
            World = world ?? throw new ArgumentNullException(nameof(world));
            State = state ?? throw new ArgumentNullException(nameof(state));
            Options = options ?? throw new ArgumentNullException(nameof(options));
            Rng = rng ?? throw new ArgumentNullException(nameof(rng));
            Movement = movement ?? throw new ArgumentNullException(nameof(movement));
            SaveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
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
