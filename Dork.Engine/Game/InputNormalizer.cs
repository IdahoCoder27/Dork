using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Dork.Engine.Game;

public static class InputNormalizer
{
    // Keep it small and predictable.
    private static readonly HashSet<string> _dropWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an"
        // later maybe: "to", "at", "on" (but NOT yet)
    };

    private static readonly Regex _multiSpace = new(@"\s+", RegexOptions.Compiled);

    public static string Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        // Lowercase, trim, collapse whitespace
        var s = _multiSpace.Replace(raw.Trim().ToLowerInvariant(), " ");

        // Tokenize
        var parts = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Drop filler words
        var filtered = parts.Where(p => !_dropWords.Contains(p));

        return string.Join(' ', filtered);
    }
}

