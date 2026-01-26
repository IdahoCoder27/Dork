using System;
using System.Collections.Generic;
using System.Linq;

namespace Dork.Engine.Model
{
    [Flags]
    public enum ItemCapability
    {
        None = 0,
        Takeable = 1 << 0,
        Usable = 1 << 1,
        Consumable = 1 << 2,
        Equipable = 1 << 3,
        Openable = 1 << 4,
        Readable = 1 << 5,
        Breakable = 1 << 6,
        Talkable = 1 << 7, // yes. DORK.
        Container = 1 << 8,
        RequiresHeld = 1 << 9,
        Hideable = 1 << 10,
        SavePoint = 1 << 11,

        // Optional future-proofing if you want to be explicit:
        // Device = 1 << 9,
        // Messaging = 1 << 10,
    }

    public enum AttachmentType
    {
        None,
        Bolted,
        Installed,
        Embedded,
        Anchored
    }

    public enum MaterialType
    {
        Unknown,
        Wood,
        Metal,
        Stone,
        Glass,
        Plastic,
        Fabric,
        Organic
    }

    /// <summary>
    /// Message lifecycle, used for phone/email/etc.
    /// Unseen: player doesn't even know it exists.
    /// Seen: player sees "you have an unread message" but hasn't opened it.
    /// Read: player opened it (knowledge is now fair game to judge).
    /// </summary>
    public enum MessageState
    {
        Unseen = 0,
        Seen = 1,
        Read = 2
    }

    public sealed class Message
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public string From { get; init; } = "Unknown";
        public string Subject { get; init; } = "";
        public string Body { get; init; } = "";

        // Runtime state
        public MessageState State { get; private set; } = MessageState.Unseen;
        public DateTimeOffset? SeenAt { get; private set; }
        public DateTimeOffset? ReadAt { get; private set; }

        public HashSet<string> Tags { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        public bool IsUnseen => State == MessageState.Unseen;
        public bool IsSeen => State == MessageState.Seen;
        public bool IsRead => State == MessageState.Read;

        public void MarkSeen(DateTimeOffset? now = null)
        {
            if (State == MessageState.Unseen)
            {
                State = MessageState.Seen;
                SeenAt = now ?? DateTimeOffset.UtcNow;
            }
        }

        public void MarkRead(DateTimeOffset? now = null)
        {
            if (State != MessageState.Read)
            {
                // Ensure SeenAt is set if we jump straight to Read
                if (State == MessageState.Unseen)
                    SeenAt = now ?? DateTimeOffset.UtcNow;

                State = MessageState.Read;
                ReadAt = now ?? DateTimeOffset.UtcNow;
            }
        }
    }

    /// <summary>
    /// Generic readable content (placards, manuals, notes).
    /// For phones, use PhoneSpec.Messages.
    /// </summary>
    public sealed class ReadableSpec
    {
        public string Title { get; init; } = "";
        public string Text { get; init; } = "";
        public bool IsSingleUse { get; init; } = false; // optional, if you want burn-after-reading notes
    }

    public sealed class Item
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";

        // Keep weight as an int if you want, but consider grams for sanity.
        public int Weight { get; init; } = 0;

        // Optional "size" pressure so containers/inventory isn't only weight-based.
        public int Bulk { get; init; } = 0;

        public HashSet<string> Tags { get; init; } = new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> Aliases { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        // What the item *can* be used for (data), not whether it *should* be allowed (rules).
        public ItemCapability Capabilities { get; init; } = ItemCapability.None;

        // Constraints / world logic
        public AttachmentType Attachment { get; init; } = AttachmentType.None; // bolted to wall, etc.
        public MaterialType Material { get; init; } = MaterialType.Unknown;

        public int? Value { get; init; } // optional economy

        // Security / access
        public bool IsStealable { get; init; } = true;
        public string? OwnerId { get; init; } // actor id, faction id, etc.

        // Optional gating: you can require an item or tag to interact.
        public int? RequiredToolItemId { get; init; }          // e.g. needs crowbar
        public string? RequiredToolTag { get; init; }          // e.g. needs "key"
        public string? RequiredSkill { get; init; }            // e.g. "Lockpicking"
        public int? RequiredSkillLevel { get; init; }          // e.g. 2

        // State that can change at runtime
        public ItemState State { get; } = new();

        // Container support (only meaningful if Capabilities has Container)
        public ContainerSpec? Container { get; init; }

        // New: readable content (notes, plaques, manuals)
        public ReadableSpec? Readable { get; init; }

        public Item(
            int id,
            string name,
            string description,
            int weight,
            bool isPortable,
            ItemCapability capabilities,
            IEnumerable<string>? aliases = null,
            IEnumerable<string>? tags = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Weight = weight;

            // Apply portability shim BEFORE assigning Capabilities
            if (isPortable) capabilities |= ItemCapability.Takeable;
            else capabilities &= ~ItemCapability.Takeable;

            Capabilities = capabilities;

            if (aliases != null)
                foreach (var a in aliases) Aliases.Add(Norm(a));

            // Always include the canonical name
            Aliases.Add(Norm(name));

            if (tags != null)
                foreach (var t in tags) Tags.Add(Norm(t));
        }

        public bool Has(ItemCapability cap) => (Capabilities & cap) == cap;

        public void Validate()
        {
            if (Id <= 0) throw new InvalidOperationException("Item.Id must be positive.");
            if (string.IsNullOrWhiteSpace(Name)) throw new InvalidOperationException($"Item {Id}: Name is required.");
            if (string.IsNullOrWhiteSpace(Description)) throw new InvalidOperationException($"Item {Id}: Description is required.");
            if (Weight < 0) throw new InvalidOperationException($"Item {Id}: Weight cannot be negative.");
            if (Bulk < 0) throw new InvalidOperationException($"Item {Id}: Bulk cannot be negative.");

            if (Has(ItemCapability.Container) && Container is null)
                throw new InvalidOperationException($"Item {Id}: Has Container capability but Container spec is null.");

            if (!Has(ItemCapability.Container) && Container is not null)
                throw new InvalidOperationException($"Item {Id}: Container spec provided but item is not marked Container.");

            if (Has(ItemCapability.Readable) && Readable is null)
                throw new InvalidOperationException($"Item {Id}: Marked Readable but Readable spec is null.");

            if (!Has(ItemCapability.Readable) && Readable is not null)
                throw new InvalidOperationException($"Item {Id}: Readable spec provided but item is not marked Readable.");


            // Validate container spec
            Container?.Validate(Id);
        }

        private static string Norm(string s) => s.Trim().ToLowerInvariant();
    }

    public sealed class ItemState
    {
        public bool IsVisible { get; set; } = true;
        public bool IsBroken { get; set; } = false;
        public bool IsHidden { get; set; }

        // Useful for Openable/Container items
        public bool IsOpen { get; set; } = false;
        public bool IsLocked { get; set; } = false;

        // Environmental chaos
        public bool IsLit { get; set; } = false;
        public bool IsWet { get; set; } = false;
        public bool IsOnFire { get; set; } = false;
    }

    public sealed class ContainerSpec
    {
        public int CapacityWeight { get; init; } = 0;
        public int CapacityBulk { get; init; } = 0;

        // Either allow-by-tag, or allow-by-item-type (you can evolve later).
        public HashSet<string> AllowedTags { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        public void Validate(int itemId)
        {
            if (CapacityWeight < 0) throw new InvalidOperationException($"Item {itemId}: CapacityWeight cannot be negative.");
            if (CapacityBulk < 0) throw new InvalidOperationException($"Item {itemId}: CapacityBulk cannot be negative.");
        }
    }
}
