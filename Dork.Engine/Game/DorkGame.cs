using Dork.Engine.Model;
using Dork.Engine.World;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dork.Engine.Game;

public sealed class DorkGame
{
    private readonly World.World _world;
    private readonly GameState _state;
    private readonly GameOptions _options;
    private readonly Random _rng = new();

    private const int PhoneItemId = 1;

    public DorkGame(World.World world, GameState state, GameOptions? options = null)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _state = state ?? throw new ArgumentNullException(nameof(state));
        _options = options ?? new GameOptions();
    }

    public GameOutput Execute(string? input)
    {
        // Normalize early (but keep original “empty” flavor)
        if (string.IsNullOrWhiteSpace(input))
            return new GameOutput("Say something. Or don't. Either way, time passes.", true, "EMPTY");

        var lower = InputNormalizer.Normalize(input);

        // First time: show the big prompt once.
        if (_state.Class == PlayerClass.None)
        {
            if (!_state.HasShownClassPrompt)
            {
                _state.MarkShownClassPrompt();
                return ShowClassPrompt();
            }
            return HandleClassGate(lower);
        }

        if (string.IsNullOrWhiteSpace(lower))
            return new GameOutput("You entered nothing. Bold strategy.", true, "EMPTY");

        // Remember whether light was on BEFORE we process, so we can message battery death correctly
        var lightWasOnAtStart = _state.PhoneLightOn;

        // Execute command
        var result = ExecuteCommand(lower);

        // Drain battery AFTER executing (single choke point)
        ApplyBatteryDrain(lower);

        // If the flashlight died as a result of this command, add a little obituary
        if (lightWasOnAtStart && !_state.PhoneLightOn && _state.PhoneBattery == 0)
        {
            result = result.Append("\nYour phone light dies with the quiet dignity of a device that’s seen your browsing history.");
        }

        return result;
    }

    private GameOutput HandleClassGate(string lower)
    {
        // Allow “classes/help” even before selection
        if (lower is "classes" or "class" or "help")
        {
            return new GameOutput(
                "Pick a class. Because apparently you need labels to fail properly.\n\n" +
                "Available:\n" +
                "- janitor\n" +
                "- intern\n" +
                "- middle manager\n" +
                "- necromancer\n\n" +
                "Type: choose <class>\n" +
                "Example: choose intern"
            );
        }

        // Empty input while gated: show prompt, but don’t error-spam
        if (string.IsNullOrWhiteSpace(lower))
            return ShowClassPrompt();

        // Accept: choose <class> OR just the class name
        var choice = lower.StartsWith("choose ") ? lower["choose ".Length..].Trim() : lower;

        var cls = choice switch
        {
            "janitor" => PlayerClass.Janitor,
            "intern" => PlayerClass.Intern,
            "middle manager" or "middlemanager" or "manager" => PlayerClass.MiddleManager,
            "necromancer" => PlayerClass.Necromancer,
            _ => PlayerClass.None
        };

        if (cls == PlayerClass.None)
        {
            // Now “No.” is earned.
            return new GameOutput(
                "Why?\n\nType `classes` to see your limited options, protagonist.",
                true,
                "BAD_CLASS"
            );
        }

        _state.Class = cls;

        var intro = cls switch
        {
            PlayerClass.Janitor => "Janitor selected. You wield keys, patience, and the quiet rage of someone who has seen things.",
            PlayerClass.Intern => "Intern selected. You have infinite optimism and zero permissions.",
            PlayerClass.MiddleManager => "Middle Manager selected. Your special ability is scheduling meetings that kill momentum.",
            PlayerClass.Necromancer => "Necromancer selected. Finally, someone qualified to debug legacy code.",
            _ => "Class selected."
        };

        // Start the game immediately with a Look()
        var look = Look();
        return new GameOutput(intro + "\n\n" + look.Text);
    }

    private GameOutput ShowClassPrompt()
    {
        return new GameOutput(
            "Welcome to DORK.\n" +
            "Pick a class. Because apparently you need labels to fail properly.\n\n" +
            "Type `classes` to see options.\n" +
            "Then: choose <class>\n" +
            "Example: choose intern"
        );
    }

    private GameOutput ExecuteCommand(string lower)
    {
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

        if (lower.StartsWith("drop "))
            return Drop(lower["drop ".Length..].Trim());

        if (lower.StartsWith("leave "))
            return Drop(lower["leave ".Length..].Trim());

        if (lower is "light" or "turn on light" or "turn on phone" or "use phone" or "use cell phone")
            return TurnOnLight();

        if (lower == "examine" || lower == "x")
            return new GameOutput("Examine what, exactly?", true, "MISSING_NOUN");

        if (lower.StartsWith("examine "))
            return Examine(lower["examine ".Length..].Trim());

        if (lower.StartsWith("x "))
            return Examine(lower["x ".Length..].Trim());

        // Direction-only commands: "out" == "go out"
        var currentRoom = _world.GetRoom(_state.CurrentRoomId);
        if (currentRoom.Exits.ContainsKey(lower))
            return Go(lower);

        return new GameOutput("Unrecognized command.", true, "UNPARSEABLE");
    }

    private GameOutput TurnOnLight()
    {
        // Require the phone to be in inventory to operate it
        if (!_state.Inventory.Contains(PhoneItemId))
            return new GameOutput("No phone. No flashlight. No hope. Maybe try picking up the obvious object next time.", true, "NO_PHONE");

        if (_state.PhoneBattery <= 0)
            return new GameOutput("You try to turn on the phone light. The phone responds with the timeless classic: dead silence.", true, "BATTERY_DEAD");

        _state.TurnPhoneLightOn();
        return new GameOutput("You turn on your phone light. Modern technology: still mostly disappointment, but bright.");
    }

    private GameOutput Look()
    {
        var room = _world.GetRoom(_state.CurrentRoomId);

        if (room.IsDark && !_state.PhoneLightOn)
        {
            return new GameOutput("It is dark.", true, "DARK");
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
                lines.Add($"- {_world.GetItem(itemId).Name}");
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
            lines.Add($"- {_world.GetItem(itemId).Name}");

        return new GameOutput(string.Join(Environment.NewLine, lines));
    }

    private GameOutput Go(string direction)
    {
        if (string.IsNullOrWhiteSpace(direction))
            return new GameOutput("Go where?", true, "MISSING_DIRECTION");

        var room = _world.GetRoom(_state.CurrentRoomId);

        if (!room.Exits.TryGetValue(direction, out var destRoomId))
            return new GameOutput($"You can't go '{direction}'.", true, "NO_EXIT");

        _state.MoveTo(destRoomId);
        return Look();
    }

    private GameOutput Take(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return new GameOutput("Take what?", true, "MISSING_NOUN");

        var room = _world.GetRoom(_state.CurrentRoomId);

        var match = room.ItemIds
            .Select(id => _world.GetItem(id))
            .FirstOrDefault(item => Matches(noun, item));

        if (match is null)
            return new GameOutput($"There is no '{noun}' here.", true, "NO_SUCH_ITEM");

        room.ItemIds.Remove(match.Id);
        _state.AddItem(match.Id);

        return new GameOutput($"Taken: {match.Name}");
    }

    private GameOutput Drop(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return new GameOutput("Drop what, exactly?", true, "MISSING_NOUN");

        var itemId = ResolveInventoryItem(noun);
        if (itemId is null)
            return new GameOutput("You are not holding that.", true, "NOT_IN_INVENTORY");

        _state.Inventory.Remove(itemId.Value);

        var room = _world.GetRoom(_state.CurrentRoomId);
        room.ItemIds.Add(itemId.Value);

        var name = _world.GetItem(itemId.Value).Name;

        var baseLine = Snark.Dropped(name, _rng);

        // Phone special flavor: light keeps draining even on ground because the light flag stays on
        if (itemId.Value == PhoneItemId && _state.PhoneLightOn)
            baseLine += "\nThe phone continues shining on the ground, like it’s trying to be the adult here.";

        return new GameOutput(baseLine);
    }

    private GameOutput Examine(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return new GameOutput("Examine what, exactly?", true, "MISSING_NOUN");

        var resolved = ResolveItem(noun);
        if (resolved is null)
            return new GameOutput("You examine… nothing. Stunning commitment to failure.", true, "NOT_FOUND");

        var item = _world.GetItem(resolved.ItemId);

        if (resolved.ItemId == PhoneItemId)
            return ExaminePhone(resolved, item);

        var locationLine = resolved.Location == ItemLocation.Inventory
            ? "It is in your possession. For now."
            : "It is here, existing quietly until you do something questionable.";

        return new GameOutput($"{item.Name}\n{item.Description}\n\n{locationLine}");
    }

    private GameOutput ExaminePhone(ResolvedItem resolved, Item item)
    {
        var lightState = _state.PhoneLightOn ? "on" : "off";

        string batteryLine = resolved.Location == ItemLocation.Inventory
            ? $"Battery: {_state.PhoneBattery}% (optimistic)."
            : _state.PhoneBattery switch
            {
                >= 75 => "Battery: strong. Like it still believes in you.",
                >= 40 => "Battery: acceptable, but the future is approaching.",
                >= 15 => "Battery: low. Your choices are catching up.",
                > 0 => "Battery: critical. This thing is one notification away from death.",
                _ => "Battery: dead. It has joined the great junk drawer in the sky."
            };

        var whereLine = resolved.Location == ItemLocation.Inventory
            ? "It rests in your hand, smudged and judgmental."
            : "It lies on the ground, doing its best impression of a useful object.";

        var extra = (resolved.Location == ItemLocation.Room && _state.PhoneLightOn)
            ? "\nThe flashlight beam cuts across the floor like it’s trying to be the adult here."
            : "";

        return new GameOutput(
            $"{item.Name}\n{item.Description}\n\n{whereLine}\n{batteryLine}\nFlashlight: {lightState}.{extra}"
        );
    }

    private void ApplyBatteryDrain(string normalizedInput)
    {
        // Drain if the light is on, regardless of where the phone is.
        if (!_state.PhoneLightOn)
            return;

        // Don’t drain for these (tune as you like)
        if (normalizedInput is "inv" or "inventory" or "i" or "help" or "classes" or "class")
            return;

        if (normalizedInput.StartsWith("examine") || normalizedInput.StartsWith("x "))
            return;

        // Simple per-action drain
        var drain = _state.Class == PlayerClass.Intern ? 2 : 1;

        _state.DrainPhoneBattery(drain);

        if (_state.PhoneBattery <= 0)
        {
            // Light shuts off when dead
            _state.TurnPhoneLightOff();
        }
    }

    private int? ResolveInventoryItem(string noun)
    {
        noun = InputNormalizer.Normalize(noun);
        if (string.IsNullOrWhiteSpace(noun))
            return null;

        foreach (var id in _state.Inventory)
        {
            var item = _world.GetItem(id);

            if (InputNormalizer.Normalize(item.Name) == noun)
                return id;

            if (item.Aliases is not null && item.Aliases.Contains(noun))
                return id;
        }

        return null;
    }

    private int? ResolveRoomItem(string noun)
    {
        noun = InputNormalizer.Normalize(noun);
        if (string.IsNullOrWhiteSpace(noun))
            return null;

        var room = _world.GetRoom(_state.CurrentRoomId);

        foreach (var id in room.ItemIds)
        {
            var item = _world.GetItem(id);

            if (InputNormalizer.Normalize(item.Name) == noun)
                return id;

            if (item.Aliases is not null && item.Aliases.Contains(noun))
                return id;
        }

        return null;
    }

    private ResolvedItem? ResolveItem(string noun)
    {
        var inv = ResolveInventoryItem(noun);
        if (inv is not null)
            return new ResolvedItem(inv.Value, ItemLocation.Inventory);

        var room = ResolveRoomItem(noun);
        if (room is not null)
            return new ResolvedItem(room.Value, ItemLocation.Room);

        return null;
    }

    private static bool Matches(string noun, Item item)
    {
        if (string.Equals(noun, item.Name, StringComparison.OrdinalIgnoreCase))
            return true;

        if (item.Name.Contains(noun, StringComparison.OrdinalIgnoreCase))
            return true;

        return item.Aliases.Any(a => string.Equals(a, noun, StringComparison.OrdinalIgnoreCase));
    }
}

// quality-of-life: append text without rewriting your whole pipeline
public static class GameOutputExtensions
{
    public static GameOutput Append(this GameOutput output, string extra)
        => new GameOutput(output.Text + extra, output.IsError, output.ErrorCode);
}
