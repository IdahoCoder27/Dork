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

        GameOutput result;

        if (lower is "look" or "l")
            result = Look();
        else if (lower is "inventory" or "inv" or "i")
            result = Inventory();
        else if (lower.StartsWith("go "))
            result = Go(lower["go ".Length..].Trim());
        else if (lower.StartsWith("take "))
            result = Take(lower["take ".Length..].Trim());
        else if (lower.StartsWith("get "))
            result = Take(lower["get ".Length..].Trim());
        else if (lower is "light" or "turn on light" or "turn on phone" or "use phone" or "use cell phone")
            result = TurnOnLight();
        else if (lower is "examine phone" or "x phone" or "inspect phone" or "check phone")
            return ExaminePhone();
        else
        {
            // Direction-only commands: "out" == "go out"
            var currentRoom = _world.GetRoom(_state.CurrentRoomId);
            if (currentRoom.Exits.ContainsKey(lower))
                result = Go(lower);
            else
                result = new GameOutput("Unrecognized command.", IsError: true, ErrorCode: "UNPARSEABLE");
        }

        // ✅ Drain battery AFTER executing the command (single choke point)
        ApplyBatteryDrain(lower);

        // If the battery just died on this command, override the result (or append if you support multiline)
        if (_state.PhoneBattery == 0 && _state.PhoneLightOn == false)
        {
            // Only report this if the player *thought* they had light
            // (i.e., it was on at the start of the command).
            // If you want that precision, store a bool before ApplyBatteryDrain.
        }

        return result;
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
    private void ApplyBatteryDrain(string normalizedInput)
    {
        // Only drain if light is on
        if (!_state.PhoneLightOn) return;

        // Don't drain for purely meta/UI commands (adjust list as you add commands)
        if (normalizedInput is "inv" or "inventory" or "help")
            return;

        if (normalizedInput.StartsWith("examine") || normalizedInput.StartsWith("x "))
            return;


        // Drain per action. Start simple: 1% per command.
        _state.DrainPhoneBattery(1);

        if (_state.PhoneBattery == 0)
        {
            _state.ClearFlag("light_on");
            // We'll return BATTERY_DEAD from Execute right after calling this.
        }
    }
    private GameOutput ExaminePhone()
    {
        const int PhoneItemId = 1; // your demo phone ID

        if (!_state.Inventory.Contains(PhoneItemId))
        {
            return new GameOutput(
                "You examine the phone you do not have. Bold. Ineffective.",
                true,
                "NO_PHONE"
            );
        }

        var battery = _state.PhoneBattery;
        var lightOn = _state.PhoneLightOn;

        var batteryDesc = battery switch
        {
            >= 80 => "Battery level: healthy. For now.",
            >= 50 => "Battery level: acceptable, but the future is approaching.",
            >= 25 => "Battery level: concerning.",
            >= 10 => "Battery level: blinking icon territory.",
            > 0 => "Battery level: critically low. You knew this was coming.",
            _ => "Battery level: dead. Emotionally and electrically."
        };

        var lightDesc = lightOn
            ? "The flashlight is currently on, burning precious electrons."
            : "The flashlight is currently off. Darkness is waiting patiently.";

        return new GameOutput(
            $"{batteryDesc}\n{lightDesc}"
        );
    }
}

