using System;

namespace Memstate.Models.Geo
{
    public class GeoPoint
    {
        public const double EarthRadiusKm = 6372.797560856;

        public GeoPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; }

        public double Longitude { get; }

        public override string ToString()
        {
            return $"(Lat:{Latitude}, Lon:{Longitude})";
        }

        public static ArcDistance Distance(GeoPoint a, GeoPoint b)
        {
            a = a.ToRadians();
            b = b.ToRadians();

            var u = Math.Sin((b.Latitude - a.Latitude) / 2);
            var v = Math.Sin((b.Longitude - b.Longitude) / 2);

            var radians = 2.0 * Math.Asin(Math.Sqrt(u * u + Math.Cos(b.Latitude) * Math.Cos(a.Latitude) * v * v));
            
            return new ArcDistance(radians);
        }
        
        public ArcDistance DistanceTo(GeoPoint other)
        {
            return Distance(this, other);
        }

        private GeoPoint ToRadians()
        {
            const double r = Math.PI / 180;
            
            return new GeoPoint(Latitude * r, Longitude * r);
        }
    }
}