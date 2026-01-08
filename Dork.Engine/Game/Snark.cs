using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
    public static class Snark
    {
        private static readonly string[] DroppedTemplates =
        {
        "Dropped: {0}. Gravity is thrilled.",
        "Dropped: {0}. The floor accepts your offering.",
        "Dropped: {0}. A bold commitment to having less.",
        "Dropped: {0}. Good. Now try not dropping yourself.",
        "Dropped: {0}. Object permanence: still a thing."
    };

        public static string Dropped(string itemName, Random rng)
        {
            var template = DroppedTemplates[rng.Next(DroppedTemplates.Length)];
            return string.Format(template, itemName);
        }
    }
}
