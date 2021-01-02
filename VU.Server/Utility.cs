using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VU.Server
{
    internal static class Utility
    {
        public static List<string> SplitStringBySpace(string str)
        {
            // Split into parts, this also works for strings that are enclosed in quotes
            return Regex.Matches(str, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();
        }
    }
}
