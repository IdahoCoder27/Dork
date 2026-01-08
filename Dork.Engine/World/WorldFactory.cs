using System;
using System.Collections.Generic;
using System.Linq;
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
            [1] = new Room
            {
                Id = 1,
                Title = "Parking Garage Elevator",
                Description = "You step into the elevator. The doors close behind you with the confidence of a system that has never met you before.",
                Exits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["out"] = 2
                },
                ItemIds = new HashSet<int> { 1 }
            },
            [2] = new Room
            {
                Id = 2,
                Title = "Meadow",
                Description = "Night. Two moons. A creek of pink water moving uphill. Reality is doing that thing where it pretends this is normal.",
                Exits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    ["in"] = 1,
                    ["dark"] = 3
                }
            },
            [3] = new Room
            {
                Id = 3,
                Title = "Pitch Black Cave",
                Description = "You can't see anything.",
                IsDark = true,
                Exits = new Dictionary<string, int>
                {
                    ["out"] = 2
                }
            }
        };

        var items = new Dictionary<int, Item>
        {
            [1] = new Item(
                id: 1,
                name: "Cell phone",
                description: "An Android-powered cellular device. It has strong opinions about battery life.",
                aliases: new[] { "phone", "cell", "android" }
            ),

            [99] = new Item(
                id: 99,
                name: "Phone charger",
                description: "A charger. Useless without electricity. Or hope.",
                aliases: new[] { "charger" }
            )
        };


        return new World(rooms, items);
    }
}

