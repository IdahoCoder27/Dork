using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Model
{
    public sealed class Exit
    {
        public int ToRoomId { get; init; }

        // Gatekeeping
        public string? RequiredFlag { get; init; }
        public string? LockedMessage { get; init; }
        public ExitType Type { get; init; } = ExitType.Normal;

        // Class gate
        public HashSet<PlayerClass>? AllowedClasses { get; init; }

        // Optional later
        public bool IsHidden { get; init; } = false;

        public bool IsAllowed(GameState state)
        {
            if (AllowedClasses is not null && AllowedClasses.Count > 0)
            {
                if (!AllowedClasses.Contains(state.Class))
                    return false;
            }

            return RequiredFlag is null || state.HasFlag(RequiredFlag);
        }
    }

    public enum ExitType
    {
        Normal,
        Elevator,
        Game
    }
}
