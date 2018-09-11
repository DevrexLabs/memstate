using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Memstate.Test
{
    [TestFixture]
    public class JournalRecordStringSerializerTests
    {
        JournalRecordStringSerializer _target;
        Dictionary<string, Type> _typeMap;

        [SetUp]
        public void SetUp()
        {
            _typeMap = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);
            _target = new JournalRecordStringSerializer(_typeMap);
        }

        [Test]
        public void HappyPath()
        {
            _typeMap["MyCommand"] = typeof(MyCommand);
            var command = new MyCommand("the data");
            var now = DateTimeOffset.Now;
            var journalRecord = new JournalRecord(32, now, command);
            var stream = new MemoryStream();
            _target.WriteObject(stream, journalRecord);

            stream.Position = 0;
            var line = new StreamReader(stream).ReadLine();
            Console.WriteLine(line);
            Assert.AreEqual("32 " + now.ToString("o") + " " + command.Id + " MyCommand the data", line);
            stream.Position = 0;

            var clone = (JournalRecord) _target.ReadObject(stream);
            Assert.AreEqual(32, clone.RecordNumber);
            Assert.AreEqual(now, clone.Written);
            Assert.AreEqual(command.Id, clone.Command.Id);

            var clonedCommand = (MyCommand)clone.Command;
            Assert.AreEqual(command.Id, clonedCommand.Id);
            Assert.AreEqual(command.MyParameter, clonedCommand.MyParameter);
        }
    }

    class MyCommand : Command<string>, ISerializable
    {

        public string MyParameter { get; private set; }

        public MyCommand(string parameter)
        {
            MyParameter = parameter;
        }

        public override void Execute(string model)
        {

        }

        void ISerializable.Restore(string data)
        {
            MyParameter = data;
        }

        void ISerializable.Save(out string typeName, out string data)
        {
            typeName = "MyCommand";
            data = MyParameter;
        }
    }
}