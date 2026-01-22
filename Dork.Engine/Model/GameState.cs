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
        /// The players choosen class which will affect humor in the game
        /// </summary>
        public PlayerClass Class { get; set; } = PlayerClass.None;

        /// <summary>
        /// Global boolean state flags (e.g., "door_open", "saw_two_moons").
        /// </summary>
        public HashSet<string> Flags { get; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Global integer counters (e.g., "turns", "failed_attempts").
        /// </summary>
        public Dictionary<string, int> Counters { get; } = new(StringComparer.OrdinalIgnoreCase);

        private const string ShownClassPromptFlag = "shown_class_prompt";
        public bool HasShownClassPrompt => HasFlag(ShownClassPromptFlag);
        public void MarkShownClassPrompt() => SetFlag(ShownClassPromptFlag);

        public bool HasPower { get; set; } = true;

        public int PhoneBattery { get; set; } = 52; // small, tense
        public bool PhonePluggedIn { get; set; }

        // Phone message state
        public int PhoneUnreadMessages { get; private set; } = 1;
        public bool PhoneMessageRead => PhoneUnreadMessages <= 0;

        // Keep it simple: one message for now
        public string PhoneMessageText { get; private set; } =
            "From: Ops Scheduling\n\nSubject: Access Approved\n\nHey —\n\nYour access was approved last minute.\n\nS.S.B.voice code is required.\nSay it clearly.\n\nDon't improvise.";

        public void MarkPhoneMessageRead()
        {
            PhoneUnreadMessages = 0;
        }

        public void SetPhoneMessage(string messageText, int unreadMessages = 1)
        {
            PhoneMessageText = messageText ?? "";
            PhoneUnreadMessages = Math.Max(0, unreadMessages);
        }


        public void TurnPhoneLightOn() => SetFlag("light_on");
        public void TurnPhoneLightOff() => ClearFlag("light_on");
        public bool PhoneLightOn => HasFlag("light_on");
        public bool IsHidden => HasFlag("player_hidden");
        public void SetHidden(bool hidden)
        {
            if (hidden) SetFlag("player_hidden");
            else ClearFlag("player_hidden");
        }

        public bool HasShownIdRecently => GetCounter("id_grace") > 0;

        /// <summary>
        /// This Falg is to determine intent
        /// </summary>
        public HashSet<string> KnowledgeFlags { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        // Optional but inevitable
        public HashSet<string> EventFlags { get; } =
            new(StringComparer.OrdinalIgnoreCase);

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

        public void SetPhoneBattery(int value)
        {
            PhoneBattery = Math.Clamp(value, 0, 100);
        }

        public void DrainPhoneBattery(int amount)
        {
            if (amount <= 0) return;
            SetPhoneBattery(PhoneBattery - amount);
        }

        public void RechargePhoneBattery(int amount)
        {
            SetPhoneBattery(PhoneBattery + amount);
        }
    }
}
