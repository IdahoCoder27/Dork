using Dork.Engine.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Systems
{
    public sealed class BatterySystem : ITurnSystem
    {
        public int Order => 500;

        public GameOutput Apply(GameContext ctx, GameOutput current)
        {
            var s = ctx.State;

            int delta = 0;

            // Drain
            if (s.PhoneLightOn)
                delta -= 1;

            // Charge
            if (s.PhonePluggedIn)
                delta += 2; // charging is faster than bleeding electrons

            if (delta == 0)
                return current;

            var before = s.PhoneBattery;
            s.PhoneBattery = Math.Clamp(s.PhoneBattery + delta, 0, 100);

            // Handle battery death
            if (before > 0 && s.PhoneBattery == 0)
            {
                s.ClearFlag("light_on");

                return new GameOutput(
                    current.Text + "\n\nThe phone goes dark. No ceremony. No apology.",
                    OutputKind.Narration,
                    current.Code
                );
            }

            // Handle full charge (optional feedback later)
            return current;
        }
    }
}
