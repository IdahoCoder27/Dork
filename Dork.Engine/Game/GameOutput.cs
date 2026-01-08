using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dork.Engine.Game
{
        public sealed record GameOutput(
        string Text,
        bool IsError = false,
        string? ErrorCode = null
    );
}
