using System;

namespace Memstate.Models.Geo
{
    public class ArcDistance : IEquatable<ArcDistance>, IComparable<ArcDistance>
    {
        public ArcDistance(double radians)
        {
            Radians = radians;
        }

        public double Radians { get; }

        public double ToKilometers()
        {
            return Radians * GeoPoint.EarthRadiusKm;
        }

        public bool Equals(ArcDistance other)
        {
            return CompareTo(other) == 0;
        }

        public int CompareTo(ArcDistance other)
        {
            return Math.Sign(Radians - other.Radians);
        }

        public override string ToString()
        {
            return $"{ToKilometers()} km";
        }
    }
}