using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Model;

namespace Dork.Engine.World;

public sealed class World
{
    public IReadOnlyDictionary<int, Room> Rooms { get; }
    public IReadOnlyDictionary<int, Item> Items { get; }

    public World(IReadOnlyDictionary<int, Room> rooms, IReadOnlyDictionary<int, Item> items)
    {
        Rooms = rooms ?? throw new ArgumentNullException(nameof(rooms));
        Items = items ?? throw new ArgumentNullException(nameof(items));

        if (Rooms.Count == 0) throw new ArgumentException("World must contain at least one room.", nameof(rooms));

        foreach (var r in Rooms.Values) r.Validate();
        foreach (var i in Items.Values) i.Validate();

        // Validate exits point to real rooms
        foreach (var r in Rooms.Values)
        {
            foreach (var exit in r.Exits)
            {
                if (!Rooms.ContainsKey(exit.Value))
                    throw new InvalidOperationException($"Room {r.Id} has exit '{exit.Key}' to missing room {exit.Value}.");
            }
        }

        // Validate room item ids exist
        foreach (var r in Rooms.Values)
        {
            foreach (var itemId in r.ItemIds)
            {
                if (!Items.ContainsKey(itemId))
                    throw new InvalidOperationException($"Room {r.Id} references missing item {itemId}.");
            }
        }
    }

    public Room GetRoom(int roomId)
        => Rooms.TryGetValue(roomId, out var room)
            ? room
            : throw new KeyNotFoundException($"Room {roomId} not found.");

    public Item GetItem(int itemId)
        => Items.TryGetValue(itemId, out var item)
            ? item
            : throw new KeyNotFoundException($"Item {itemId} not found.");
}

