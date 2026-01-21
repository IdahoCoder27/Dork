using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.World
{
    public sealed class Guard
    {
        public int Id { get; init; }
        public string Name { get; set; } = "Guard";
        public int CurrentRoomId { get; set; }

        public IReadOnlyList<int> Route { get; init; } = Array.Empty<int>();
        public int RouteIndex { get; set; } = 0;

        public GuardState State { get; set; } = GuardState.Patrol;

        // Optional: “he’s coming” behavior
        public int? TargetRoomId { get; set; }
    }

    public enum GuardState
    {
        Patrol,
        Investigate
    }
}
