using System.Text.RegularExpressions;

namespace Memstate.Configuration
{
    static class RegexExtensions
    {
        public static bool TryMatch(this Regex regex, string input, out Match match)
        {
            match = regex.Match(input);
            return match.Success;
        }
    }

}
