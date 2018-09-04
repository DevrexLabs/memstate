using Memstate.Models.Geo;

namespace Memstate.Models.Redis
{
    public class NamedGeoPoint
    {
        public NamedGeoPoint(string name, GeoPoint point)
        {
            Name = name;
            Point = point;
        }

        public string Name { get; }

        public GeoPoint Point { get; }
    }
}