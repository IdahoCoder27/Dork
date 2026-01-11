using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Model
{
    public sealed class DeathRecord
    {
        public string CauseCode { get; init; } = "";
        public string Description { get; init; } = "";
        public string LocationId { get; init; } = "";
        public DateTime Timestamp { get; init; }

        // Optional humiliation enhancers
        public string? ToolUsed { get; init; }
        public string? Target { get; init; }
        public bool WasCompletelyAvoidable { get; init; } = true;
    }

    public enum DeathCategory
    {
        Environmental,
        Fire,
        Radiation,
        Cryogenic,
        Gravity,
        Hubris,
        Experimental
    }

}
