using System;
using System.Collections.Generic;

namespace Memstate.Models.Graph
{
    public partial class GraphModel
    {
        [Serializable]
        public abstract class Item : IComparable<Item>
        {
            public readonly long Id;
            public readonly string Label;
            public readonly SortedDictionary<string, object> Props;

            protected Item(long id, string label)
            {
                Id = id;
                Label = label;
                Props = new SortedDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            public object Get(string key)
            {
                object result;
                Props.TryGetValue(key, out result);
                return result;
            }

            public void Set(string key, object value)
            {
                Props[key] = value;
            }

            public int CompareTo(Item other)
            {
                return Math.Sign(Id - other.Id);
            }

            public override bool Equals(object obj)
            {
                return obj != null
                && obj.GetType() == GetType() //Edge == Node should always be false
                && ((Item)obj).Id == Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }

    }
}
