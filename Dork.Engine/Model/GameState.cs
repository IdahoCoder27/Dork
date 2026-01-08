using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Model
{
    public sealed class GameState
    {
        /// <summary>
        /// Current room/location ID the player occupies.
        /// </summary>
        public int CurrentRoomId { get; private set; }

        /// <summary>
        /// Item IDs currently held by the player.
        /// </summary>
        public HashSet<int> Inventory { get; } = new();

        /// <summary>
        /// Global boolean state flags (e.g., "door_open", "saw_two_moons").
        /// </summary>
        public HashSet<string> Flags { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Global integer counters (e.g., "turns", "failed_attempts").
        /// </summary>
        public Dictionary<string, int> Counters { get; } = new(StringComparer.OrdinalIgnoreCase);

        public GameState(int startingRoomId)
        {
            if (startingRoomId <= 0)
                throw new ArgumentOutOfRangeException(nameof(startingRoomId), "Room IDs must be positive.");

            CurrentRoomId = startingRoomId;
        }

        public void MoveTo(int roomId)
        {
            if (roomId <= 0)
                throw new ArgumentOutOfRangeException(nameof(roomId), "Room IDs must be positive.");

            CurrentRoomId = roomId;
        }

        public bool HasItem(int itemId) => Inventory.Contains(itemId);

        public void AddItem(int itemId)
        {
            if (itemId <= 0)
                throw new ArgumentOutOfRangeException(nameof(itemId), "Item IDs must be positive.");

            Inventory.Add(itemId);
        }

        public bool RemoveItem(int itemId) => Inventory.Remove(itemId);

        public bool HasFlag(string flag) => Flags.Contains(NormalizeKey(flag));

        public bool SetFlag(string flag)
        {
            var key = NormalizeKey(flag);
            return Flags.Add(key);
        }

        public bool ClearFlag(string flag)
        {
            var key = NormalizeKey(flag);
            return Flags.Remove(key);
        }

        public int GetCounter(string counter) => Counters.TryGetValue(NormalizeKey(counter), out var v) ? v : 0;

        public int IncrementCounter(string counter, int delta = 1)
        {
            var key = NormalizeKey(counter);
            var next = GetCounter(key) + delta;
            Counters[key] = next;
            return next;
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null/empty.", nameof(key));

            return key.Trim();
        }
    }
}
