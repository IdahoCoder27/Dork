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


        /// <summary>
        /// Direction -> destination room id (e.g. "north" => 2)
        /// </summary>
        public Dictionary<string, int> Exits { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Item IDs currently in this room.
        /// </summary>
        public HashSet<int> ItemIds { get; init; } = new();

        public void Validate()
        {
            if (Id <= 0) throw new InvalidOperationException("Room.Id must be positive.");
            if (string.IsNullOrWhiteSpace(Title)) throw new InvalidOperationException($"Room {Id}: Title is required.");
            if (string.IsNullOrWhiteSpace(Description)) throw new InvalidOperationException($"Room {Id}: Description is required.");

            foreach (var kvp in Exits)
            {
                if (string.IsNullOrWhiteSpace(kvp.Key))
                    throw new InvalidOperationException($"Room {Id}: Exit direction cannot be empty.");
                if (kvp.Value <= 0)
                    throw new InvalidOperationException($"Room {Id}: Exit '{kvp.Key}' must point to a positive room id.");
            }
        }
    }
}
