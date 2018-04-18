using System.IO;

namespace Memstate
{
    public static class SerializerExtensions
    {
        public static T Clone<T>(this ISerializer serializer, T graph)
        {
            return (T) serializer.Deserialize(serializer.Serialize(graph));
        }

        public static byte[] Serialize(this ISerializer serializer, object graph)
        {
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, graph);

                stream.Position = 0;

                return stream.ToArray();
            }
        }

        public static object Deserialize(this ISerializer serializer, byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes) {Position = 0})
            {
                return serializer.ReadObject(stream);
            }
        }
    }
}