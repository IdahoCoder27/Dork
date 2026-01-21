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
    public IReadOnlyList<Guard> Guards { get; }

    public World(IReadOnlyDictionary<int, Room> rooms, IReadOnlyDictionary<int, Item> items, IReadOnlyList<Guard>? guards)
    {
        Rooms = rooms ?? throw new ArgumentNullException(nameof(rooms));
        Items = items ?? throw new ArgumentNullException(nameof(items));
        Guards = guards ?? Array.Empty<Guard>();

        if (Rooms.Count == 0) throw new ArgumentException("World must contain at least one room.", nameof(rooms));

        foreach (var r in Rooms.Values) r.Validate();
        foreach (var i in Items.Values) i.Validate();

        // Validate exits point to real rooms
        foreach (var room in Rooms.Values)
        {
            foreach (var (dir, exit) in room.Exits)
            {
                if (!Rooms.ContainsKey(exit.ToRoomId))
                    throw new InvalidOperationException(
                        $"Room {room.Id} has exit '{dir}' to missing room {exit.ToRoomId}."
                    );
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

    public void AdvanceGuards(GameState state)
    {
        foreach (var guard in Guards)
        {
            if (guard.State != GuardState.Patrol)
                continue;

            guard.RouteIndex = (guard.RouteIndex + 1) % guard.Route.Count;
            guard.CurrentRoomId = guard.Route[guard.RouteIndex];
        }

    }

    public Guard? DetectPlayer(GameState state)
        => Guards.FirstOrDefault(g => g.CurrentRoomId == state.CurrentRoomId);

    public bool AreAdjacent(int a, int b)
    {
        var ra = GetRoom(a);
        return ra.Exits.Values.Any(e => e.ToRoomId == b);
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

