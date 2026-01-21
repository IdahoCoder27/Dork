using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Dork.Engine.Model;

namespace Dork.Engine.World;

public static class WorldFactory
{
    public static World CreateDemoWorld()
    {
        var rooms = new Dictionary<int, Room>
        {
            // ------------------------
            // START AREA (outside world)
            // ------------------------
            [1] = new Room
            {
                Id = 1,
                Title = "Parking Garage Elevator",
                Description =
                    "You step into the elevator. The doors close behind you with the confidence of a system that has never met you before.\n" +
                    "You are in a small elevator. A small plaque lies above an old panel. The panel waits for you to make a decision.",
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["out"] = new Exit { ToRoomId = 2 },

                    // Keeping this synonym if you like it, but it should *also* go out to the garage.
                    ["field"] = new Exit { ToRoomId = 2 },

                    // IMPORTANT: this must go to the SSB elevator, not the meadow.
                    ["ssb"] = new Exit
                    {
                        ToRoomId = 4,
                        RequiredFlag = "elevator_ssb_unlocked",
                        LockedMessage =
                            "You try to go to S.S.B.\n" +
                            "The elevator refuses.\n\n" +
                            "ACCESS DENIED.\n" +
                            "Try voice authorization. Or literacy."
                    }
                },
                ItemIds = new HashSet<int> { 1, 10, 11 }
            },

            [2] = new Room
            {
                Id = 2,
                Title = "Parking Garage",
                Description = "You are in a parking garage. Several cars are here. Most of them look expensive enough to contain regret.",
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["in"] = new Exit { ToRoomId = 1 },
                    ["out"] = new Exit { ToRoomId = 3 },
                }
            },

            [3] = new Room
            {
                Id = 3,
                Title = "Meadow",
                Description = "Night. Two moons. A creek of pink water moving uphill. Reality is doing that thing where it pretends this is normal.",
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["in"] = new Exit { ToRoomId = 2 },
                    ["garage"] = new Exit { ToRoomId = 2 },
                    ["parking garage"] = new Exit { ToRoomId = 2 },
                }
            },

            // ------------------------
            // SSB (Floor 1)
            // ------------------------
            [4] = new Room
            {
                Id = 4,
                Title = "Elevator - Sub-Sass Basement",
                Description =
                    "The elevator doors open with reluctant professionalism.\n" +
                    "A clean corridor waits outside, like it’s disappointed you made it this far.",
                // Up to you: if you want the arrival to be lit, leave it false.
                // If you want the arrival to be dark and force phone-light mechanics, set true.
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["out"] = new Exit { ToRoomId = 5 },
                    ["in"] = new Exit { ToRoomId = 1 }, // lets them bail back to the garage elevator if you want
                }
            },

            [5] = new Room
            {
                Id = 5,
                Title = "SSB Lobby",
                Description =
                    "The lobby is narrow and aggressively clean.\n\n" +
                    "Fluorescent lights buzz with the patience of institutional architecture. " +
                    "The floor reflects your shape just enough to feel judgmental.\n\n" +
                    "A security desk sits against one wall. Its monitors glow with camera feeds from somewhere else.\n\n" +
                    "Several reinforced doors interrupt the perimeter, each with a small status light and a label that assumes you belong here.\n\n" +
                    "A heavy fire door rests in a recessed alcove. It has the look of something designed to be used only when everything has already gone wrong.",
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["in"] = new Exit { ToRoomId = 4 },

                    // Door alcoves (separate rooms so players can examine without clutter)
                    ["west"] = new Exit { ToRoomId = 8 },
                    ["east"] = new Exit { ToRoomId = 9 },

                    // Stairwell access
                    ["up"] = new Exit { ToRoomId = 6 },
                    ["stairwell"] = new Exit { ToRoomId = 6 },
                },
                ItemIds = new HashSet<int> { 13, 14 } // desk, terminal live here
            },

            [6] = new Room
            {
                Id = 6,
                Title = "Stairwell Door",
                Description =
                    "A heavy fire-rated door marked UP.\n\n" +
                    "A small sign beneath it reads:\n" +
                    "STAIR ACCESS PER FIRE CODE. DO NOT BLOCK.\n\n" +
                    "The handle looks like it’s been used by people who don’t get paid enough to care.",
                // Door area is lit enough from lobby spill, usually.
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["out"] = new Exit { ToRoomId = 5 },
                    ["up"] = new Exit { ToRoomId = 7 },
                    ["in"] = new Exit { ToRoomId = 7 }, // convenience
                },
                ItemIds = new HashSet<int> { 15 } // stairwell door item
            },

            [7] = new Room
            {
                Id = 7,
                Title = "Stairwell",
                Description =
                    "Concrete. Paint. Echoes.\n\n" +
                    "Your footsteps sound louder than they should, like the building wants to make sure someone notices.\n\n" +
                    "Up leads deeper into offices. Down leads back to the lobby.",
                // This is a good place to be dark later if you want stealth/guard mechanics.
                IsDark = true,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["down"] = new Exit { ToRoomId = 6 },
                    ["out"] = new Exit { ToRoomId = 6 },
                    // Floor 2 later:
                    ["up"] = new Exit { ToRoomId = 24 }
                }
            },

            // West side: Door 1 & Door 2 alcoves (Floor 1)
            [8] = new Room
            {
                Id = 8,
                Title = "West Corridor",
                Description =
                    "A short corridor with two reinforced doors.\n\n" +
                    "Both have red indicator lights. Both look allergic to your existence.",
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["back"] = new Exit { ToRoomId = 5 },
                    ["lobby"] = new Exit { ToRoomId = 5 },
                },
                // Put only one “door” item here unless you’ve made door-specific items.
                ItemIds = new HashSet<int> { 12 }
            },

            // East side: Door 3–5 corridor (Floor 1)
            [9] = new Room
            {
                Id = 9,
                Title = "East Corridor",
                Description =
                    "A longer corridor with multiple secured entrances.\n\n" +
                    "Every label is corporate. Every lock is sincere.",
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["back"] = new Exit { ToRoomId = 5 },
                    ["lobby"] = new Exit { ToRoomId = 5 },
                },
                ItemIds = new HashSet<int> { 12 } // reuse reinforced door description for now
            },

            [20] = new Room
            {
                Id = 20,
                Title = "Floor 2 Lobby",
                Description =
                    "The second-floor lobby is the same shape as downstairs, but it feels different.\n" +
                    "Less public. More procedural.\n\n" +
                    "The lighting is steady and bright enough to make bad decisions look intentional.\n" +
                    "A few doors line the perimeter with numbered labels that assume you know what they mean.\n\n" +
                    "A trash can sits in the open like an accusation.",
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    // back to stairwell door
                    ["up"] = new Exit { ToRoomId = 24 },
                    ["stairwell"] = new Exit { ToRoomId = 24 },
                    ["back"] = new Exit { ToRoomId = 24 },

                    // corridors
                    ["west"] = new Exit { ToRoomId = 21 },
                    ["east"] = new Exit { ToRoomId = 22 },

                    // restricted room (placeholder gate)
                    ["operations"] = new Exit
                    {
                        ToRoomId = 23,
                        AllowedClasses = new HashSet<PlayerClass>
                        {
                            PlayerClass.Janitor,
                            PlayerClass.MiddleManager
                        },
                        LockedMessage =
                            "You approach the Operations Support door.\n\n" +
                            "A small sign reads: AUTHORIZED STAFF ONLY.\n" +
                            "The lock reads: ALSO YOU.\n\n" +
                            "Access denied."
                    },
                    ["ops"] = new Exit
                    {
                        ToRoomId = 23,
                        AllowedClasses = new HashSet<PlayerClass>
                        {
                            PlayerClass.Janitor,
                            PlayerClass.MiddleManager
                        },
                        LockedMessage =
                            "Operations Support remains inaccessible.\n" +
                            "It does this professionally."
                    }
                },
                ItemIds = new HashSet<int> { 16 } // trash can
            },

            [21] = new Room
            {
                Id = 21,
                Title = "Floor 2 West Corridor",
                Description =
                    "A short corridor with two reinforced doors.\n\n" +
                    "Their labels are faded, their locks are not.\n" +
                    "The little status lights glow steadily, as if the building is quietly pleased with itself.",
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["back"] = new Exit { ToRoomId = 20 },
                    ["lobby"] = new Exit { ToRoomId = 20 },
                    ["east"] = new Exit { ToRoomId = 20 }
                },
                ItemIds = new HashSet<int> { 12 } // reuse Reinforced Door item for now
            },

            [22] = new Room
            {
                Id = 22,
                Title = "Floor 2 East Corridor",
                Description =
                    "A longer corridor with multiple secured entrances.\n\n" +
                    "This side feels more occupied. Not by people.\n" +
                    "By policy.\n\n" +
                    "Somewhere behind these doors, someone is paid to say \"no\" for a living.",
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["back"] = new Exit { ToRoomId = 20 },
                    ["lobby"] = new Exit { ToRoomId = 20 },
                    ["west"] = new Exit { ToRoomId = 20 }
                },
                ItemIds = new HashSet<int> { 12 } // reuse Reinforced Door item
            },

            [23] = new Room
            {
                Id = 23,
                Title = "Operations Support",
                Description =
                    "The room smells like toner, overheated plastic, and quiet resentment.\n\n" +
                    "A row of terminals hums along the wall. A whiteboard is filled with acronyms.\n" +
                    "None of them explain anything.\n\n" +
                    "This is where problems go to become tickets.\n" +
                    "And where tickets go to be ignored.",
                IsDark = false,
                HasPower = true,
                GateRuleId = "OPS_SUPPORT_RESTRICTED",
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["out"] = new Exit { ToRoomId = 20 },
                    ["lobby"] = new Exit { ToRoomId = 20 },
                    ["back"] = new Exit { ToRoomId = 20 },
                },
                ItemIds = new HashSet<int> { 17 }
            },

            [24] = new Room
            {
                Id = 24,
                Title = "Floor 2 Stairwell Door",
                Description =
                    "A heavy fire-rated door marked FLOOR 2.\n\n" +
                    "A small sign reads:\n" +
                    "STAIR ACCESS PER FIRE CODE. DO NOT BLOCK.\n\n" +
                    "The handle is worn from use.\n" +
                    "Not by you. By people who know what they're doing.",
                IsDark = false,
                Exits = new Dictionary<string, Exit>(StringComparer.OrdinalIgnoreCase)
                {
                    ["out"] = new Exit { ToRoomId = 20 },
                    ["down"] = new Exit { ToRoomId = 7 },
                    ["in"] = new Exit { ToRoomId = 7 }, // convenience
                    ["stairwell"] = new Exit { ToRoomId = 7 }
                },
                ItemIds = new HashSet<int> { 15 } // reuse Stairwell Door item
            },
        };

        var items = new Dictionary<int, Item>
        {
            [1] = new Item(
                id: 1,
                name: "Cell phone",
                description: "An Android-powered cellular device. It has strong opinions about battery life.",
                weight: 1,
                isPortable: true,
                capabilities: ItemCapability.Takeable | ItemCapability.Breakable | ItemCapability.Readable,
                aliases: new[] { "phone", "cell", "android" }
            )
            {
                Phone = new PhoneSpec
                {
                    Messages = new List<Message>
                    {
                        new Message
                        {
                            From = "Ops Scheduling",
                            Subject = "Access Approved",
                            Body = "Hey —\n\nYour access was approved last minute.\nS.S.B. voice code is LASAGNA.\n\nDon’t ask.\nJust say it clearly.",
                            Tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "auth", "ssb", "security" }
                        }
                    }
                }
            },

            [10] = new Item(
                id: 10,
                name: "Elevator Panel",
                description:
                    "A grimy panel with two buttons: FIELD and S.S.B., What did you expect?\n" +
                    "A small grille sits above the buttons.\n" +
                    "A faded label reads: VOICE AUTH REQUIRED.",
                weight: 0,
                isPortable: false,
                capabilities: ItemCapability.None,
                aliases: new[] { "panel", "buttons", "button", "elevator panel" }
            ),

            [11] = new Item(
                id: 11,
                name: "Elevator Plaque",
                description:
                    "A shiny plaque with the words 'AUTHORIZED PERSONNEL ONLY.\n" +
                    "ACCESS VIA S.S.B.\n" +
                    "CODEWORD ON FILE.' emblazoned upon it.",
                weight: 0,
                isPortable: false,
                capabilities: ItemCapability.None,
                aliases: new[] { "plaque", "elevator plaque" }
            ),

            [12] = new Item(
                id: 12,
                name: "Reinforced Door",
                description:
                    "The door is steel-reinforced, electronically locked, and absolutely uninterested in your presence.\n\n" +
                    "A red indicator light glows steadily. Whatever is behind it is not for you. Not tonight.",
                weight: 0,
                isPortable: false,
                capabilities: ItemCapability.None,
                aliases: new[] { "door", "reinforced door" }
            ),

            [13] = new Item(
                id: 13,
                name: "Security Desk",
                description:
                    "The desk is immaculate. No personal items. No coffee. No clutter.\n\n" +
                    "Multiple monitors display camera feeds from elsewhere in the facility. None of them show this lobby.",
                weight: 0,
                isPortable: false,
                capabilities: ItemCapability.None,
                aliases: new[] { "desk", "security desk" }
            ),

            [14] = new Item(
                id: 14,
                name: "Access Terminal",
                description:
                    "The terminal is powered on, displaying a single message:\n\n" +
                    "AFTER-HOURS ACCESS RESTRICTED\n\n" +
                    "AUTHORIZED PERSONNEL ONLY",
                weight: 0,
                isPortable: false,
                capabilities: ItemCapability.None,
                aliases: new[] { "terminal", "computer", "access terminal" }
            ),

            [15] = new Item(
                id: 15,
                name: "Stairwell Door",
                description:
                    "A heavy fire-rated door marked UP.\n\n" +
                    "A small sign beneath it reads:\n" +
                    "STAIR ACCESS PER FIRE CODE. DO NOT BLOCK.",
                weight: 0,
                isPortable: false,
                capabilities: ItemCapability.None,
                aliases: new[] { "stairwell door", "fire access door", "fire door" }
            ),

            [16] = new Item(
                id: 16,
                name: "Trash Can",
                description:
                    "A standard office trash can.\n\n" +
                    "It is empty. Suspiciously empty.\n" +
                    "Like it’s waiting for you to contribute something embarrassing.",
                weight: 0,
                isPortable: false,
                capabilities: ItemCapability.None,
                aliases: new[] { "trash", "trashcan", "can", "bin" }
            ),

                        [17] = new Item(
                id: 17,
                name: "Operations Console",
                description:
                    "A workstation with multiple monitors and an authentication prompt.\n\n" +
                    "The screen reads:\n" +
                    "PLEASE BADGE IN\n\n" +
                    "A little help text underneath says:\n" +
                    "DO NOT CALL I.T. AFTER HOURS.",
                weight: 0,
                isPortable: false,
                capabilities: ItemCapability.None,
                aliases: new[] { "console", "workstation", "terminal", "ops console", "computer" }
            ),

            [99] = new Item(
                id: 99,
                name: "Phone charger",
                description: "A charger. Useless without electricity. Or hope.",
                weight: 1,
                isPortable: true,
                capabilities: ItemCapability.Takeable | ItemCapability.Breakable,
                aliases: new[] { "charger" }
            )
        };

        var guards = new List<Guard>
{
            new Guard
            {
                Id = 1,
                Name = "Facility Guard",
                CurrentRoomId = 20, // Floor 2 Lobby
                Route = new List<int>
                {
                    20, 21, 20, 22, 20, 24, 20, 23, 20
                },
                RouteIndex = 0
            }
        };

        return new World(rooms, items, guards);
    }
}
