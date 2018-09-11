using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Memstate
{

    /// <summary>
    /// An ISerializer that converts journalrecords to and from a single line
    /// of text with the format:
    ///  RECORDNUMBER COMMAND_ID DATE COMMAND_TYPE COMMAND_DATA
    /// </summary>
    public class JournalRecordStringSerializer : ISerializer
    {
        readonly Dictionary<string, Type> _typeMap;


        public JournalRecordStringSerializer(Dictionary<string, Type> typeMap)
        {
            Ensure.NotNull(typeMap, nameof(typeMap));
            _typeMap = typeMap;
        }

        public object ReadObject(Stream stream)
        {
            var line = new StreamReader(stream).ReadLine();
            return JournalRecordFromLine(line);
        }

        public IEnumerable<T> ReadObjects<T>(Stream stream)
        {
            var reader = new StreamReader(stream);
            while (true)
            {
                var line = reader.ReadLine();
                if (line == null) break;
                else yield return (T)(object)JournalRecordFromLine(line);
            }
        }

        private JournalRecord JournalRecordFromLine(string line)
        {
            var splitPoint = line.IndexOf(' ');
            var data = line.Split(new char[] { ' ' }, 5);

            var recordNumber = Int64.Parse(data[0]);
            var recorded = DateTimeOffset.Parse(data[1]);
            var commandId = Guid.Parse(data[2]);
            var type = _typeMap[data[3]];
            var command = (Command) FormatterServices.GetUninitializedObject(type);
            command.Id = commandId;
            ((ISerializable)command).Restore(data[4]);

            return new JournalRecord(recordNumber, recorded, command);
        }

        public void WriteObject(Stream stream, object @object)
        {
            var type = @object.GetType();
            var writer = new StreamWriter(stream);
            if (type == typeof(JournalRecord))
            {
                writer.WriteLine(JournalRecordToString((JournalRecord)@object));
            }
            else if (type == typeof(JournalRecord[]))
            {
                foreach(var record in (JournalRecord[])@object)
                {
                    writer.WriteLine(JournalRecordToString(record));
                }
            }
            else throw new InvalidOperationException("Serializer only supports JournalRecord and JournalRecord[]");
            writer.Flush();
        }

        private string JournalRecordToString(JournalRecord record)
        {
            var selfSerializingCommand = (ISerializable)record.Command;
            selfSerializingCommand.Save(out var typeIdentifier, out string commandAsString);
            return String.Join(" ",
                               record.RecordNumber,
                               record.Written.ToString("o"),
                               record.Command.Id,
                               typeIdentifier,
                               commandAsString);
        }
    }
}