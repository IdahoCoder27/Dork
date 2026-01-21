using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Systems
{
    public sealed class GuardSystem : ITurnSystem
    {
        public int Order => 99;

        public GameOutput Apply(GameContext ctx, GameOutput current)
        {
            // HARD GATE: guard logic only runs at night, while in building, and on floor 2.
            if (!ShouldRun(ctx))
                return current;

            // Not implemented yet, but we also refuse to crash the game.
            // When you implement patrol/encounters later, it goes here.

            return current;
        }

        private static bool ShouldRun(GameContext ctx)
        {
            // 1) In building?
            // Pick ONE source of truth. For now we use a flag because it's cheap.
            // Set this flag when the player enters the building.
            if (!ctx.State.HasFlag("in_building"))
                return false;

            // 2) At night?
            // Same deal: set this from your story clock later.
            if (!ctx.State.HasFlag("is_night"))
                return false;

            // 3) On floor 2?
            // Easiest: store current floor as a counter.
            // Set it when using stairs. (1 or 2)
            if (ctx.State.GetCounter("floor") != 2)
                return false;

            return true;
        }
    }
}
