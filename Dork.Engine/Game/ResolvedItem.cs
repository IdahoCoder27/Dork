using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
    public enum ItemLocation
    {
        Inventory,
        Room
    }

    public sealed record ResolvedItem(int ItemId, ItemLocation Location);
}
