using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
    public sealed class SaveGame
    {
        public int CurrentRoomId { get; set; }
        public List<int> InventoryItemIds { get; set; } = new();
        public HashSet<string> Flags { get; set; } = new();
    }
}
