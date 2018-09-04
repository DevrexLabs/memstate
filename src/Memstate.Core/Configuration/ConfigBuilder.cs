﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Memstate.Configuration
{

    public class ConfigBuilder
    {
        private Dictionary<string, string> _args;

        public ConfigBuilder()
        {
            _args = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        }

        public Config Build()
        {
            return new Config(_args);
        }

        public ConfigBuilder AddCommandLine(string[] args, string prefix = "Memstate")
        {
            var pattern = "^(?<key>" + prefix + @"(:\w+)+)=(?<val>.+)";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (var arg in args)
            {
                Match match = regex.Match(arg);
                if (match.Success)
                {
                    _args[match.Groups["key"].Value] = match.Groups["val"].Value;
                }
            }
            return this;
        }

        public ConfigBuilder AddEnvironmentVariables(string prefix = "Memstate")
        {
            var vars = Environment.GetEnvironmentVariables();
            foreach (String key in vars.Keys)
            {
                if (PrefixIsMatch(key, prefix))
                {
                    _args[key.Replace("_", ":")] = (string)vars[key];
                }
            }
            return this;
        }

        public ConfigBuilder AddIniFiles()
        {
            var env = Environment.GetEnvironmentVariable("ENV");
            if (!String.IsNullOrEmpty(env))
            {
                env = env.ToLower();
                var file = $"memstate.{env}.ini";
                IniFile.MergeIfExists(file, _args);
            }
            IniFile.MergeIfExists("memstate.ini", _args);
            return this;
        }

        internal static bool PrefixIsMatch(string key, string prefix)
        {
            return key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
