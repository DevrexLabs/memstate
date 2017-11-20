using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Memstate.Models.Geo
{
    public class GeoSpatialIndex<T> : IDictionary<T, GeoPoint>
    {
        private const double EarthCircumference = GeoPoint.EarthRadiusKm * 2 * Math.PI;
        private readonly SortedDictionary<T, Entry> _entries;
        private readonly SortedSet<Entry> _byLatitude;
        private readonly SortedSet<Entry> _byLongitude;

        public GeoSpatialIndex()
        {
            _entries = new SortedDictionary<T, Entry>();
            _byLatitude = new SortedSet<Entry>(new DelegateComparer((a, b) => a.Point.Latitude.CompareTo(b.Point.Latitude)));
            _byLongitude = new SortedSet<Entry>(new DelegateComparer((a, b) => a.Point.Longitude.CompareTo(b.Point.Longitude)));
        }

        public int Count => _entries.Count;

        public bool IsReadOnly => false;

        public GeoPoint this[T key]
        {
            get
            {
                if (_entries.ContainsKey(key))
                {
                    return _entries[key].Point;
                }

                throw new KeyNotFoundException();
            }

            set
            {
                if (ContainsKey(key))
                {
                    Remove(key);
                }

                Add(key, value);
            }
        }

        public ICollection<T> Keys => _entries.Keys;

        public ICollection<GeoPoint> Values => _entries.Values.Select(x => x.Point).ToArray();

        public IEnumerable<KeyValuePair<T, ArcDistance>> WithinRadius(GeoPoint origin, double radiusInKm)
        {
            var radiusInDegreesLatitude = radiusInKm / EarthCircumference * 360 * 1.005;

            var minLat = Math.Max(-90, origin.Latitude - radiusInDegreesLatitude);
            var maxLat = Math.Min(90, origin.Latitude + radiusInDegreesLatitude);

            var south = new Entry(minLat, 0);
            var north = new Entry(maxLat, 0);

            var absMaxLat = Math.Max(Math.Abs(minLat), Math.Abs(maxLat));

            var distanceInDegreesLongitude = radiusInDegreesLatitude / Math.Cos(absMaxLat * Math.PI / 180);

            var minLong = origin.Longitude - distanceInDegreesLongitude;
            var maxLong = origin.Longitude + distanceInDegreesLongitude;

            return _byLatitude.GetViewBetween(south, north)
                .Intersect(LongitudeRange(minLong, maxLong))
                .Select(entry => new KeyValuePair<T, ArcDistance>(entry.Item, GeoPoint.Distance(entry.Point, origin)))
                .Where(kvp => kvp.Value.ToKilometers() <= radiusInKm)
                .OrderBy(kvp => kvp.Value);
        }

        public IEnumerator<KeyValuePair<T, GeoPoint>> GetEnumerator()
        {
            return _entries.Values.Select(x => new KeyValuePair<T, GeoPoint>(x.Item, x.Point)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<T, GeoPoint> item)
        {
            if (_entries.ContainsKey(item.Key))
            {
                throw new InvalidOperationException("Key already exists");
            }

            var entry = new Entry(item.Key, item.Value);

            _entries.Add(item.Key, entry);
            _byLatitude.Add(entry);
            _byLongitude.Add(entry);
        }

        public void Clear()
        {
            _entries.Clear();
            _byLatitude.Clear();
            _byLongitude.Clear();
        }

        public bool Contains(KeyValuePair<T, GeoPoint> item)
        {
            _entries.TryGetValue(item.Key, out var entry);

            return entry != null && entry.Point == item.Value;
        }

        public void CopyTo(KeyValuePair<T, GeoPoint>[] array, int arrayIndex)
        {
            foreach (var pair in this)
            {
                array[arrayIndex++] = pair;
            }
        }

        public bool Remove(KeyValuePair<T, GeoPoint> item)
        {
            return Contains(item) && Remove(item.Key);
        }

        public bool ContainsKey(T key)
        {
            return _entries.ContainsKey(key);
        }

        public void Add(T key, GeoPoint value)
        {
            if (_entries.ContainsKey(key))
            {
                throw new InvalidOperationException("Key already exists");
            }

            var item = new Entry(key, value);

            _byLatitude.Remove(item);
            _byLatitude.Add(item);

            _byLongitude.Remove(item);
            _byLongitude.Add(item);
        }

        public bool Remove(T key)
        {
            if (!_entries.TryGetValue(key, out var entry))
            {
                return false;
            }

            _entries.Remove(key);
            _byLatitude.Remove(entry);
            _byLongitude.Remove(entry);

            return true;
        }

        public bool TryGetValue(T key, out GeoPoint value)
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                value = entry.Point;

                return true;
            }

            value = null;

            return false;
        }

        private IEnumerable<Entry> LongitudeRange(double from, double to)
        {
            if (from < -180)
            {
                var result = _byLongitude.GetViewBetween(new Entry(0, from + 360), new Entry(0, 180));

                result.UnionWith(LongitudeRange(-180, to));

                return result;
            }

            if (to > 180)
            {
                var result = _byLongitude.GetViewBetween(new Entry(0, from - 360), new Entry(0, -180));

                result.UnionWith(LongitudeRange(from, 180));

                return result;
            }

            return _byLongitude.GetViewBetween(new Entry(0, from), new Entry(0, to));
        }

        private class DelegateComparer : IComparer<Entry>
        {
            private readonly Func<Entry, Entry, int> _comparer;

            public DelegateComparer(Func<Entry, Entry, int> comparer)
            {
                _comparer = comparer;
            }

            public int Compare(Entry a, Entry b)
            {
                if (a == null)
                {
                    throw new ArgumentNullException(nameof(a));
                }

                if (b == null)
                {
                    throw new ArgumentNullException(nameof(b));
                }

                var result = _comparer.Invoke(a, b);

                return result != 0 ? result : Comparer<T>.Default.Compare(a.Item, b.Item);
            }
        }

        private class Entry
        {
            public readonly GeoPoint Point;
            public readonly T Item;

            public Entry(T item, GeoPoint point)
            {
                Item = item;
                Point = point;
            }

            public Entry(double latitude, double longitude)
            {
                Point = new GeoPoint(latitude, longitude);
            }
        }
    }
}