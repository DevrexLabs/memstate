using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Memstate.Configuration
{
    internal static class IniFile
    {
        internal static List<KeyValuePair<string, string>> Parse(IEnumerable<string> lines)
        {
            //list instead of dictionary because
            //we need them in order in case of duplicates
            var result = new List<KeyValuePair<string, string>>();

            var sectionMatcher = new Regex(@"^\[\s*(?<section>[A-Z0-9:]+)\s*\]$", RegexOptions.IgnoreCase);
            var keyValueMatcher = new Regex(@"^(?<key>[A-Z0-9:]+)\s*=\s*(?<value>.*)$", RegexOptions.IgnoreCase);

            //Last section matched
            var currentSection = "";
            foreach (var line in lines.Select(s => s.Trim()))
            {
                //Skip comments and empty lines
                if (string.IsNullOrEmpty(line) || line[0] == '#') continue;

                if (sectionMatcher.TryMatch(line, out var sectionMatch))
                {
                    currentSection = sectionMatch.Groups["section"].Value;
                }
                else if (keyValueMatcher.TryMatch(line, out var kvpMatch))
                {
                    var localKey = kvpMatch.Groups["key"].Value;
                    var key = PrependSection(currentSection, localKey);
                    var value = kvpMatch.Groups["value"].Value;
                    result.Add(new KeyValuePair<string, string>(key, value));
                }
                else
                {
                    throw new Exception("Invalid ini file line: " + line);
                }
            }
            return result;
        }

        private static string PrependSection(string section, string key)
        {
            if (!string.IsNullOrEmpty(section))
            {
                key = section + ":" + key;
            }
            return key;
        }

        internal static void MergeIfExists(string file, Dictionary<string, string> args)
        {
            if (!File.Exists(file)) return;
            var lines = File.ReadAllLines(file);
            foreach (var pair in Parse(lines))
            {
                args[pair.Key] = pair.Value;
            }
        }
    }
}
