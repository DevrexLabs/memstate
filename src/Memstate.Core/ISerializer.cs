using System.Collections.Generic;
using System.IO;

namespace Memstate
{
    public interface ISerializer
    {
        void WriteObject(Stream stream, object @object);

        object ReadObject(Stream stream);

        IEnumerable<T> ReadObjects<T>(Stream stream);
    }

    public interface ISerializable
    {
        void Save(out string typeName, out string data);
        void Restore(string data);
    }

    public interface IJournalRecordFormatter
    {
        string Stringify(JournalRecord journalRecord);
        JournalRecord Parse(string data);
    }
}