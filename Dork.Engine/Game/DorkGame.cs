using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Model;
using Dork.Engine.World;

namespace Dork.Engine.Game;

public sealed class DorkGame
{
    private readonly World.World _world;
    private readonly GameState _state;
    private readonly GameOptions _options;
    private const int PhoneItemId = 1;

    public DorkGame(World.World world, GameState state, GameOptions? options = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _options = options ?? new GameOptions();
    }

    public GameOutput Execute(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new GameOutput("Say something. Or don't. Either way, time passes.", IsError: true, ErrorCode: "EMPTY");

        var lower = InputNormalizer.Normalize(input);
        if (string.IsNullOrWhiteSpace(lower))
            return new GameOutput("You entered nothing. Bold strategy.", true, "EMPTY");

        if (lower is "look" or "l")
            return Look();

        if (lower is "inventory" or "inv" or "i")
            return Inventory();

        if (lower.StartsWith("go "))
            return Go(lower["go ".Length..].Trim());

        if (lower.StartsWith("take "))
            return Take(lower["take ".Length..].Trim());

        if (lower.StartsWith("get "))
            return Take(lower["get ".Length..].Trim());

        if (lower is "light" or "turn on light" or "turn on phone" or "use phone" or "use cell phone")
            return TurnOnLight();

        // Direction-only commands: "out" == "go out"
        var currentRoom = _world.GetRoom(_state.CurrentRoomId);
        if (currentRoom.Exits.ContainsKey(lower))
        {
            return Go(lower);
        }

        return new GameOutput("Unrecognized command.", IsError: true, ErrorCode: "UNPARSEABLE");

    }

    private GameOutput TurnOnLight()
    {
        if (!_state.Inventory.Contains(PhoneItemId))
            return new GameOutput("You have nothing that produces light. Try possessing objects first.", true, "NO_LIGHT_SOURCE");

        _state.SetFlag("light_on");
        return new GameOutput("You turn on your phone light. Modern technology: still mostly disappointment, but bright.");
    }


    private GameOutput Look()
    {
        var room = _world.GetRoom(_state.CurrentRoomId);

        if (room.IsDark && !_state.HasFlag("light_on"))
        {
            return new GameOutput(
                "It is dark.",
                IsError: true,
                ErrorCode: "DARK"
            );
        }

        var lines = new List<string>
        {
            room.Title,
            room.Description
        };

        if (room.ItemIds.Count > 0)
        {
            lines.Add("");
            lines.Add("You see:");
            foreach (var itemId in room.ItemIds.OrderBy(x => x))
            {
                var item = _world.GetItem(itemId);
                lines.Add($"- {item.Name}");
            }
        }

        if (_options.ShowExits && room.Exits.Count > 0)
        {
            lines.Add("");
            lines.Add("Exits:");
            foreach (var kvp in room.Exits.OrderBy(k => k.Key))
                lines.Add($"- {kvp.Key}");
        }

        return new GameOutput(string.Join(Environment.NewLine, lines));
    }

    private GameOutput Inventory()
    {
        if (_state.Inventory.Count == 0)
            return new GameOutput("Inventory: (empty)");

        var lines = new List<string> { "Inventory:" };
        foreach (var itemId in _state.Inventory.OrderBy(x => x))
        {
            var item = _world.GetItem(itemId);
            lines.Add($"- {item.Name}");
        }

        return new GameOutput(string.Join(Environment.NewLine, lines));
    }

    private GameOutput Go(string direction)
    {
        if (string.IsNullOrWhiteSpace(direction))
            return new GameOutput("Go where?", IsError: true, ErrorCode: "MISSING_DIRECTION");

        var room = _world.GetRoom(_state.CurrentRoomId);

        if (!room.Exits.TryGetValue(direction, out var destRoomId))
            return new GameOutput($"You can't go '{direction}'.", IsError: true, ErrorCode: "NO_EXIT");

        _state.MoveTo(destRoomId);
        return Look();
    }

    private GameOutput Take(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return new GameOutput("Take what?", IsError: true, ErrorCode: "MISSING_NOUN");

        var room = _world.GetRoom(_state.CurrentRoomId);

        // match by name/alias in items present in the room
        var match = room.ItemIds
            .Select(id => _world.GetItem(id))
            .FirstOrDefault(item => Matches(noun, item));

        if (match is null)
            return new GameOutput($"There is no '{noun}' here.", IsError: true, ErrorCode: "NO_SUCH_ITEM");

        room.ItemIds.Remove(match.Id);
        _state.AddItem(match.Id);

        return new GameOutput($"Taken: {match.Name}");
    }

    private static bool Matches(string noun, Item item)
    {
        if (string.Equals(noun, item.Name, StringComparison.OrdinalIgnoreCase))
            return true;

        // allow "cell phone" vs "phone" with a contains check
        if (item.Name.Contains(noun, StringComparison.OrdinalIgnoreCase))
            return true;

        return item.Aliases.Any(a => string.Equals(a, noun, StringComparison.OrdinalIgnoreCase));
    }
}

