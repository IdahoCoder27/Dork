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

    private enum SpeechVolume { Normal, Yell }
    private static readonly HashSet<string> MessageNouns = new(StringComparer.OrdinalIgnoreCase)
    {
        "message", "messages", "unread", "unread message", "unread messages", "inbox"
    };

    private const int PhoneItemId = 1;
    private const int PanelItemId = 10;
    private const int PlaqueItemId = 11;

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
        // Light intent normalization: strip "player:" and leading "i " so casual inputs still work.
        // This is NOT an LLM. This is just removing human fluff.
        var cmd = NormalizeIntent(lower);

        // First time: show the big prompt once.
        if (_state.Class == PlayerClass.None)
        {
            if (!_state.HasShownClassPrompt)
            {
                _state.MarkShownClassPrompt();
                return ShowClassPrompt();
            }
            return HandleClassGate(cmd);
        }

        if (string.IsNullOrWhiteSpace(cmd))
            return new GameOutput("You entered nothing. Bold strategy.", true, "EMPTY");

        // Remember whether light was on BEFORE we process, so we can message battery death correctly
        var lightWasOnAtStart = _state.PhoneLightOn;

        // Execute command
        var result = ExecuteCommand(cmd);

        // Drain battery AFTER executing (single choke point)
        ApplyBatteryDrain(cmd);

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

        if (lower.StartsWith("press "))
            return Press(lower["press ".Length..].Trim());

        if (lower.StartsWith("push "))
            return Press(lower["push ".Length..].Trim());

        // Legacy habits die hard.
        if (lower.StartsWith("enter ") || lower.StartsWith("type "))
            return new GameOutput("There is no keypad. The panel wants voice authorization. Try: say <codeword>.", true, "VOICE_REQUIRED");

        // Reading
        if (lower.StartsWith("read "))
            return Read(lower["read ".Length..].Trim());
        if (lower is "read" or "r")
            return new GameOutput("Read what, exactly?", true, "MISSING_NOUN");

        // Speech / voice input
        if (lower.StartsWith("say "))
            return Say(lower["say ".Length..].Trim(), volume: SpeechVolume.Normal);
        if (lower.StartsWith("speak "))
            return Say(lower["speak ".Length..].Trim(), volume: SpeechVolume.Normal);
        if (lower.StartsWith("yell "))
            return Say(lower["yell ".Length..].Trim(), volume: SpeechVolume.Yell);
        if (lower.StartsWith("shout "))
            return Say(lower["shout ".Length..].Trim(), volume: SpeechVolume.Yell);

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

    private GameOutput Read(string noun)
    {
        noun = InputNormalizer.Normalize(noun);

        if (string.IsNullOrWhiteSpace(noun))
            return new GameOutput("Read what, exactly?", true, "MISSING_NOUN");

        // If they say "read message", treat it as content-intent, not object-aliasing.
        if (MessageNouns.Contains(noun))
            return ReadMessage(); // no noun needed

        // Otherwise: read an explicit thing.
        var resolved = ResolveItem(noun);
        if (resolved is null)
            return new GameOutput($"You can't find '{noun}' to read. Try reality.", true, "NOT_FOUND");

        var item = _world.GetItem(resolved.ItemId);

        // Generic readable items (manuals, plaques, etc.)
        if (item.Readable is not null)
        {
            _state.KnowledgeFlags.Add($"ReadItem:{item.Id}");
            var title = string.IsNullOrWhiteSpace(item.Readable.Title) ? item.Name : item.Readable.Title;
            return new GameOutput($"{title}\n\n{item.Readable.Text}");
        }

        // If flagged readable but no content, that's a content-authoring bug.
        if (item.Has(ItemCapability.Readable))
            return new GameOutput("It is technically readable. Unfortunately, it contains nothing.", true, "EMPTY_READABLE");

        return new GameOutput($"You can't read the {item.Name}.", true, "NOT_READABLE");
    }

    private GameOutput ReadMessage()
    {
        // Prefer: inventory message devices first (phone later becomes terminal, pager, etc.)
        var invDeviceId = _state.Inventory
            .Select(id => _world.GetItem(id))
            .FirstOrDefault(i => i.Phone is not null) // message-capable device
            ?.Id;

        if (invDeviceId is not null)
            return ReadMessagesFromDevice(invDeviceId.Value);

        // Optional: allow room devices if you want (terminals on desks, wall kiosks).
        // If you *don't* want this, just delete this block and it's inventory-only.
        var room = _world.GetRoom(_state.CurrentRoomId);
        var roomDeviceId = room.ItemIds
            .Select(id => _world.GetItem(id))
            .FirstOrDefault(i => i.Phone is not null)
            ?.Id;

        if (roomDeviceId is not null)
            return ReadMessagesFromDevice(roomDeviceId.Value, mustBeHeld: true);

        return new GameOutput("No readable messages available. Peace at last.", true, "NO_MESSAGES");
    }

    private GameOutput ReadMessagesFromDevice(int itemId, bool mustBeHeld = false)
    {
        var item = _world.GetItem(itemId);

        if (item.Phone is null)
            return new GameOutput("You stare at it, hoping for messages. It offers you disappointment.", true, "NO_MESSAGES");

        // If this is a “personal device” like a phone, require holding it.
        // You can generalize later via tags or a property like item.RequiresHeldToRead.
        if (mustBeHeld || itemId == PhoneItemId)
        {
            if (!_state.Inventory.Contains(item.Id))
            {
                return new GameOutput(
                    $"You can't read the {item.Name} from the floor. Hands exist. Use them.",
                    true,
                    "ITEM_NOT_HELD"
                );
            }
        }

        var msg = item.Phone.ReadNext();
        if (msg is null)
            return new GameOutput("No unread messages.");

        var codeword = TryExtractCodeword(msg.Body);
        if (!string.IsNullOrWhiteSpace(codeword))
        {
            _state.KnowledgeFlags.Add("KnowsSSBCodeword");
            _state.KnowledgeFlags.Add($"SSBCodeword:{codeword}");
        }

        var header = $"From: {msg.From}\nSubject: {msg.Subject}";
        return new GameOutput($"{header}\n\n{msg.Body}");
    }

    private GameOutput Say(string text, SpeechVolume volume)
    {
        text = InputNormalizer.Normalize(text);
        if (string.IsNullOrWhiteSpace(text))
            return new GameOutput("Say what, exactly?", true, "MISSING_SPEECH");

        var room = _world.GetRoom(_state.CurrentRoomId);

        // Elevator voice auth: this is the only place where 'say' matters right now.
        if (room.Id == 1)
        {
            // Require the panel to exist in the room, otherwise people can voice-authenticate the concept of elevators.
            if (!room.ItemIds.Contains(PanelItemId))
                return new GameOutput("You speak into the air. The air does not grant you clearance.", true, "NO_MIC");

            // Hard-coded codeword for now. Later you can compare against the extracted one stored in KnowledgeFlags.
            // Keep it simple until you have Wall-of-Shame scoring.
            if (text == "lasagna")
            {
                _state.SetFlag("elevator_ssb_unlocked");
                return new GameOutput(
                    "You say, \"lasagna.\"\n\n" +
                    "There is a pause.\n" +
                    "The panel beeps. Not approvingly. Just… accurately.\n" +
                    "S.S.B. lights up.\n\n"
                );
            }

            // Wrong word: punish confidence if they clearly should know better.
            var knows = PlayerKnows("KnowsSSBCodeword");
            var extra = knows
                ? "\n\nSomewhere, a system records your voice and your poor reading comprehension."
                : "\n\nNothing happens. The panel waits.";

            if (volume == SpeechVolume.Yell)
                extra += "\nAlso, you yelled at a machine. That probably looked great.";

            return new GameOutput($"You say, \"{text}.\"{extra}", true, "BAD_VOICE_CODE");
        }

        // Everywhere else: speech is mostly roleplay. For now.
        if (volume == SpeechVolume.Yell)
            return new GameOutput($"You yell \"{text}\". The environment absorbs it without comment.");

        return new GameOutput($"You say \"{text}\". Nobody answers.");
    }

    private bool PlayerKnows(string flag)
        => _state.KnowledgeFlags.Contains(flag);

    private static string NormalizeIntent(string normalized)
    {
        // Already normalized input comes in here.
        var s = normalized.Trim();

        if (s.StartsWith("player:"))
            s = s["player:".Length..].Trim();

        // Common human fluff: "I ..." / "I want to ..." / "I would like to ..."
        if (s.StartsWith("i want to "))
            s = s["i want to ".Length..].Trim();
        else if (s.StartsWith("i would like to "))
            s = s["i would like to ".Length..].Trim();
        else if (s.StartsWith("i "))
            s = s["i ".Length..].Trim();

        return s;
    }

    private static string? TryExtractCodeword(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;

        // Super-literal extraction: look for "code is X".
        // Later you can make this robust. Right now, this is enough to power the elevator.
        var lower = body.ToLowerInvariant();
        var needle = "code is ";
        var idx = lower.IndexOf(needle, StringComparison.Ordinal);
        if (idx < 0) return null;

        var start = idx + needle.Length;
        var tail = body[start..].Trim();
        if (tail.Length == 0) return null;

        // Take the first token-ish word, strip punctuation.
        var token = tail.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (token is null) return null;
        token = token.Trim().Trim('"', '\'', '.', ',', ';', ':', '!', '?');
        return string.IsNullOrWhiteSpace(token) ? null : token.ToLowerInvariant();
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

        direction = InputNormalizer.Normalize(direction);

        var room = _world.GetRoom(_state.CurrentRoomId);

        if (!room.Exits.TryGetValue(direction, out var exit))
            return new GameOutput($"You can't go '{direction}'.", true, "NO_EXIT");

        if (!exit.IsAllowed(_state))
            return new GameOutput(exit.LockedMessage ?? "ACCESS DENIED.", true, "EXIT_LOCKED");

        _state.MoveTo(exit.ToRoomId);
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

        if (!match.IsPortable)
        {
            return new GameOutput(
                $"You try to take the {match.Name}.\n" +
                "It does not budge.\n\n" +
                "This is architecture, not loot.",
                true,
                "NOT_PORTABLE"
            );
        }

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
        else if (resolved.ItemId == PlaqueItemId)
            return ExaminePlaque(resolved, item);

        var locationLine = resolved.Location == ItemLocation.Inventory
            ? "It is in your possession. For now."
            : "It is here, existing quietly until you do something questionable.";

        return new GameOutput($"{item.Name}\n{item.Description}\n\n{locationLine}");
    }

    private GameOutput ExaminePhone(ResolvedItem resolved, Item item)
    {
        var lightState = _state.PhoneLightOn ? "on" : "off";

        // If this is a real phone (PhoneSpec present), seeing the phone reveals that messages exist.
        var unreadLine = "";
        if (item.Phone is not null)
        {
            // Checking the phone marks unseen messages as seen, but not read.
            item.Phone.MarkAllSeen();
            var unread = item.Phone.UnreadCount;
            if (unread > 0)
                unreadLine = $"\nYou have {unread} unread message{(unread == 1 ? "" : "s")}.";
        }

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
            $"{item.Name}\n{item.Description}{unreadLine}\n\n{whereLine}\n{batteryLine}\nFlashlight: {lightState}.{extra}"
        );
    }

    private GameOutput ExaminePlaque(ResolvedItem resolved, Item item)
    {
        return new GameOutput($"{item.Name}\n{item.Description}");
    }

    private GameOutput Press(string noun)
    {
        if (string.IsNullOrWhiteSpace(noun))
            return new GameOutput("Press what, exactly?", true, "MISSING_NOUN");

        noun = InputNormalizer.Normalize(noun);

        // Special case: elevator sub-buttons like "ssb" / "field"
        // These are NOT items, they are controls on the panel.
        if (noun is "ssb" or "subbasement" or "sub basement" or "sb" or "2" or "field" or "f" or "1")
            return PressElevatorButton(noun);

        // Otherwise, press an actual item in scope
        var target = FindItemInScope(noun);
        if (target is null)
            return new GameOutput($"You press '{noun}'. Nothing happens.\nBecause nothing is there.", true, "NO_TARGET");

        return PressItem(target);
    }

    private Item? FindItemInScope(string token)
    {
        token = InputNormalizer.Normalize(token);

        var room = _world.GetRoom(_state.CurrentRoomId);

        // Room items first
        foreach (var id in room.ItemIds)
        {
            var it = _world.GetItem(id);
            if (it.Aliases.Contains(token)) return it;
        }

        // Inventory items next (optional, but useful)
        foreach (var id in _state.Inventory)
        {
            var it = _world.GetItem(id);
            if (it.Aliases.Contains(token)) return it;
        }

        return null;
    }

    private GameOutput PressElevatorButton(string noun)
    {
        var room = _world.GetRoom(_state.CurrentRoomId);

        // Only works if you're actually in the elevator
        if (room.Id != 1)
            return new GameOutput("You press at the air. The air remains unimpressed.", true, "NO_PANEL");

        // FIELD button
        if (noun is "field" or "f" or "1")
        {
            // Do NOT Go("field") unless field is a real exit you want to allow by movement.
            // Better: teleport / scripted movement.
            return ForceMoveTo(3);
        }

        // SSB button
        if (noun is "ssb" or "subbasement" or "sub basement" or "sb" or "2")
        {
            if (!_state.HasFlag("elevator_ssb_unlocked"))
                return new GameOutput(
                    "You press S.S.B.\n" +
                    "Nothing happens.\n\n" +
                    "The panel flashes: ACCESS DENIED.\n" +
                    "Apparently even elevators have standards.\n\n" +
                    "Maybe examine the panel. Or your life choices.",
                    true,
                    "LOCKED"
                );

            // Teleport. Not an exit.
            return ForceMoveTo(4);
        }

        return new GameOutput("The button does nothing. Possibly because it isn't real.", true, "BAD_BUTTON");
    }

    private GameOutput ForceMoveTo(int roomId)
    {
        _state.MoveTo(roomId);
        return Look();
    }

    private GameOutput PressItem(Item item)
    {
        return item.Id switch
        {
            10 => new GameOutput("You press the elevator panel. Try a button: FIELD or S.S.B.", true, "PANEL_HINT"),
            _ => new GameOutput($"You press the {item.Name}.\nIt feels pressed.", true, "PRESSED")
        };
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
