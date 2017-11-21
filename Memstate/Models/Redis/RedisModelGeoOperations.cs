using System.Collections.Generic;
using System.Linq;
using Memstate.Models.Geo;

namespace Memstate.Models.Redis
{
    public partial class RedisModel
    {
        public int GeoAdd(string key, params NamedGeoPoint[] points)
        {
            var geo = GetGeoSpatialDictionary(key);

            var result = 0;

            foreach (var point in points)
            {
                if (!geo.ContainsKey(point.Name))
                {
                    result++;
                }

                geo[point.Name] = point.Point;
            }

            return result;
        }

        public ArcDistance GeoDist(string key, string member1, string member2)
        {
            var geo = GetGeoSpatialDictionary(key);

            if (geo.TryGetValue(member1, out var point1) && geo.TryGetValue(member2, out var point2))
            {
                return point1.DistanceTo(point2);
            }

            return null;
        }

        public GeoPoint[] GeoPos(string key, params string[] fields)
        {
            var geo = GetGeoSpatialDictionary(key);

            var result = new List<GeoPoint>(fields.Length);

            foreach (var field in fields)
            {
                geo.TryGetValue(field, out var point);

                result.Add(point);
            }

            return result.ToArray();
        }

        public KeyValuePair<NamedGeoPoint, ArcDistance>[] GeoRadius(
            string key,
            GeoPoint center,
            double radiusKm,
            int count = int.MaxValue)
        {
            var geo = GetGeoSpatialDictionary(key);

            return geo.WithinRadius(center, radiusKm)
                .Select(p => new KeyValuePair<NamedGeoPoint, ArcDistance>(new NamedGeoPoint(p.Key, geo[p.Key]), p.Value))
                .Take(count)
                .ToArray();
        }

        public KeyValuePair<NamedGeoPoint, ArcDistance>[] GeoRadiusByMember(
            string key,
            string member,
            double radiusKm,
            int count = int.MaxValue)
        {
            var center = GeoPos(key, member)[0];

            if (center == null)
            {
                throw new KeyNotFoundException("No such member: " + member);
            }

            return GeoRadius(key, center, radiusKm, count);
        }

        private GeoSpatialIndex<string> GetGeoSpatialDictionary(string key, bool create = false)
        {
            return As(key, create, () => new GeoSpatialIndex<string>());
        }
    }
}