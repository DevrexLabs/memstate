using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Memstate
{
	public class BinaryFormatterAdapter : BinarySerializer
	{
		readonly BinaryFormatter _formatter = new BinaryFormatter();

		public override object ReadObject(Stream stream)
			=> _formatter.Deserialize(stream);

		public override void WriteObject(Stream stream, object @object)
			=> _formatter.Serialize(stream, @object);
	}
}