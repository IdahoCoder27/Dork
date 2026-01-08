using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Model
{
    public sealed class Item
    {
        public int Id { get; init; }
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";

        /// <summary>
        /// Optional extra names players might type (e.g. "phone", "cell", "android")
        /// </summary>
        public List<string> Aliases { get; init; } = new();

        public void Validate()
        {
            if (Id <= 0) throw new InvalidOperationException("Item.Id must be positive.");
            if (string.IsNullOrWhiteSpace(Name)) throw new InvalidOperationException($"Item {Id}: Name is required.");
            if (string.IsNullOrWhiteSpace(Description)) throw new InvalidOperationException($"Item {Id}: Description is required.");
        }
    }
}
