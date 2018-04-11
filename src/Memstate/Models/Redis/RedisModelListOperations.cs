using System;
using System.Collections.Generic;

namespace Memstate.Models.Redis
{
    public partial class RedisModel
    {
        public string LIndex(string key, int index)
        {
            var list = GetList(key);

            index += index <= 0 ? list.Count : 0;

            return index >= 0 && index < list.Count ? list[index] : null;
        }

        public int LPush(string key, params string[] values)
        {
            return NPush(key, head: true, values: values);
        }

        public int RPush(string key, params string[] values)
        {
            return NPush(key, head: false, values: values);
        }

        public int NPush(string key, bool head, params string[] values)
        {
            var list = GetList(key, create: true);

            var index = head ? 0 : list.Count;

            list.InsertRange(index, values);

            var result = list.Count;

            if (list.Count == 0)
            {
                _structures.Remove(key);
            }

            return result;
        }

        public int LInsert(string key, string pivot, string value, bool before = true)
        {
            var list = GetList(key);

            if (list == null)
            {
                return 0;
            }

            var index = list.IndexOf(pivot);

            if (index == -1)
            {
                return -1;
            }

            if (!before)
            {
                index++;
            }

            list.Insert(index, value);

            return list.Count;
        }

        public int LLength(string key)
        {
            var list = GetList(key);

            return list?.Count ?? 0;
        }

        public string LPop(string key)
        {
            var list = GetList(key);

            if (list == null || list.Count == 0)
            {
                return null;
            }

            var result = list[0];

            list.RemoveAt(0);

            return result;
        }

        public string RPop(string key)
        {
            var list = GetList(key);

            if (list == null || list.Count == 0)
            {
                return null;
            }

            var result = list[list.Count - 1];

            list.RemoveAt(list.Count - 1);

            return result;
        }

        public void LSet(string key, int index, string value)
        {
            var list = GetList(key);

            if (list == null)
            {
                throw new KeyNotFoundException();
            }

            if (index < 0)
            {
                index += list.Count;
            }

            if (index < 0 || index >= list.Count)
            {
                throw new IndexOutOfRangeException();
            }

            list[index] = value;
        }

        private List<string> GetList(string key, bool create = false)
        {
            return As(key, create, () => new List<string>());
        }
    }
}