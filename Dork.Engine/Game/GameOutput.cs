using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
    public enum OutputKind { Narration, Prompt, Error }

    public sealed record GameOutput(string Text, OutputKind Kind = OutputKind.Narration, string? Code = null);

}
