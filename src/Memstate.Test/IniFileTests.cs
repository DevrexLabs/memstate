using System.Collections.Generic;
using Memstate.Configuration;
using NUnit.Framework;

namespace Memstate.Test
{
    [TestFixture]
    public class IniFileTests
    {
        Config _config;
        
        [SetUp]
        public void TestSetup()
        {
            var validLines = validConfig.Split("\n");
            var args = new Dictionary<string, string>();
            foreach(KeyValuePair<string,string> line in IniFile.Parse(validLines))
            {
                args[line.Key] = line.Value;  
            }
            _config = new Config(args);
        }

        [Test, TestCaseSource(nameof(Keys))]
        public void NoSection(string key)
        {
            Assert.True(_config.Data.ContainsKey(key));
            Assert.AreEqual(key, _config.Data[key]);
        }

        [Test]
        public void ValueWithWhitespace()
        {
            Assert.AreEqual("white space", _config.Data["whitespace"]);
        }

        [Test, TestCaseSource(nameof(CompositeKeys))]
        public void Sections(string key)
        {
            Assert.True(key.EndsWith(_config.Data[key], System.StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<string> CompositeKeys()
        {
            yield return "Section1:key1";
            yield return "Section2:key1";
            yield return "Section3:key1";
            yield return "Composite:Section:key1";
        }

        private static IEnumerable<string> Keys()
        {
            for (int i = 1; i < 8; i++) yield return "key" + i; 
        }

        const string validConfig = @"
key1=key1
# whitepace, including trailing!
key2 = key2
key3= key3
key4 =key4
  key5 = key5
#tabs
key6    =   key6  
    key7 = key7

   # comment with leading spaces
    # comment with leading tab

#whitespace within value
whitespace = white space


#section
[Section1]
key1=key1

# section with whitespace
[ Section2 ]
key1=key1

#section with leading whitspace
    [Section3]
    key1=key1

# empty section
[EmptySection]

# Composite key section
[Composite:Section]
key1=key1
";
    }
}