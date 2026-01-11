using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Model
{
    public sealed class Room
    {
        public int Id { get; init; }
        public string Title { get; init; } = "";
        public string Description { get; init; } = ""; 
        public bool IsDark { get; init; }
        public bool HasPower { get; init; }

        /// <summary>
        /// Direction -> destination room id (e.g. "north" => 2)
        /// </summary>
        public Dictionary<string, Exit> Exits { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// If there are rules to the room, they lie here
        /// </summary>
        public string? GateRuleId { get; init; }


        /// <summary>
        /// Item IDs currently in this room.
        /// </summary>
        public HashSet<int> ItemIds { get; init; } = new();

        public void Validate()
        {
            if (Id <= 0)
                throw new InvalidOperationException("Room.Id must be positive.");

            foreach (var (dir, exit) in Exits)
            {
                if (string.IsNullOrWhiteSpace(dir))
                    throw new InvalidOperationException($"Room {Id}: Exit direction cannot be empty.");

                if (exit is null)
                    throw new InvalidOperationException($"Room {Id}: Exit '{dir}' is null.");

                if (exit.ToRoomId <= 0)
                    throw new InvalidOperationException(
                        $"Room {Id}: Exit '{dir}' must point to a positive room id."
                    );
            }
        }

    }
}
