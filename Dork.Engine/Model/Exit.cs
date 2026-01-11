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
        public string? RequiredFlag { get; init; }          // simplest form
        public string? LockedMessage { get; init; }

        // Optional later
        public bool IsHidden { get; init; } = false;

        public bool IsAllowed(GameState state) =>
            RequiredFlag is null || state.HasFlag(RequiredFlag);
    }
}
