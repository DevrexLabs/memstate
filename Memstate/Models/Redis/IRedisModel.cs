using System;
using System.Collections.Generic;
using Memstate.Models.Geo;

namespace Memstate.Models.Redis
{
    public interface IRedisModel : IDisposable
    {
        int Delete(params string[] keys);
        void Clear();
        int KeyCount();
        bool Exists(string key);
        bool Expire(string key, DateTime at);
        RedisModel.KeyType Type(string key);
        int Append(string key, string value);
        void Set(string key, string value);
        bool SetUnlessExists(string key, string value);
        string Get(string key);
        string GetRange(string key, int start, int end);
        int BitCount(string key, int firstBit = 0, int lastBit = int.MaxValue);
        bool GetBit(string key, int offset);
        bool SetBit(string key, int index, bool value = true);
        int BitOp(BitOperator op, string key, params string[] sourceKeys);
        int BitPos(string key, bool value, int firstBit = 0, int lastBit = int.MaxValue);
        string GetSet(string key, string value);
        long DecrementBy(string key, long delta);
        long Decrement(string key);
        long Increment(string key);
        long IncrementBy(string key, long delta);
        string[] Keys(string regex = "^.*$");
        string[] MGet(params string[] keys);
        void MSet(params string[] keyValuePairs);
        void HMSet(string keys, params string[] keyValuePairs);
        string[] HMGet(string key, params string[] fields);
        int StrLength(string key);
        string RandomKey();
        void Rename(string key, string newKey);
        bool Persist(string key);
        int PurgeExpired();
        DateTime? Expires(string key);
        string[] GetExpiredKeys();
        bool ZAdd(string key, string member, double score);
        int ZAdd(string key, params KeyValuePair<string, double>[] items);
        int ZAdd(string key, IDictionary<string, double> membersAndScores);
        int ZAdd(string key, params string[] scoreAndMembersInterlaced);
        int ZCard(string key);
        int ZCount(string key, double min, double max);
        double ZIncrementBy(string key, double increment, string member);
        int ZInterStore(string destination, string[] keys, double[] weights = null, RedisModel.AggregateType aggregateType = RedisModel.AggregateType.Sum);
        string[] ZRange(string key, int start = 0, int stop = -1);
        SortedSet<ZSetEntry> ZRangeWithScores(string key, int start = 0, int stop = -1);
        string[] ZRangeByScore(string key, double min, double max, int skip = 0, int take = int.MaxValue);
        SortedSet<ZSetEntry> ZRangeByScoreWithScores(string key, double min, double max, int skip = 0, int take = int.MaxValue);
        int? ZRank(string key, string member);
        int ZRemove(string key, params string[] members);
        int ZRemoveRangeByRank(string key, int first, int last);
        int ZRemoveRangeByScore(string key, double min, double max);
        double? ZScore(string key, string member);
        string[] ZReverseRange(string key, int start, int stop);
        SortedSet<ZSetEntry> ZReverseRangeWithScores(string key, int start = 0, int stop = 0);

        string[] ZReverseRangeByScore(
            string key,
            double min = double.MinValue,
            double max = double.MaxValue,
            int skip = 0,
            int take = int.MaxValue);

        SortedSet<ZSetEntry> ZReverseRangeByScoreWithScores(
            string key,
            double min = double.MinValue,
            double max = double.MaxValue,
            int skip = 0,
            int take = int.MaxValue);

        int? ZReverseRank(string key, string member);

        int ZUnionStore(
            string destination,
            string[] keys,
            double[] weights = null,
            RedisModel.AggregateType aggregateType = RedisModel.AggregateType.Sum);

        int GeoAdd(string key, params NamedGeoPoint[] points);
        ArcDistance GeoDist(string key, string member1, string member2);
        GeoPoint[] GeoPos(string key, params string[] fields);

        KeyValuePair<NamedGeoPoint, ArcDistance>[] GeoRadius(
            string key,
            GeoPoint center,
            double radiusKm,
            int count = int.MaxValue);

        KeyValuePair<NamedGeoPoint, ArcDistance>[] GeoRadiusByMember(
            string key,
            string member,
            double radiusKm,
            int count = int.MaxValue);

        int SAdd(string key, params string[] values);
        int SCard(string key);
        string[] SDiff(string key, params string[] setsToSubtract);
        int SDiffStore(string destination, string key, params string[] setsToSubtract);
        string[] SInter(string key, params string[] keys);
        int SInterStore(string destination, string key, params string[] keys);
        bool SIsMember(string key, string value);
        string[] SMembers(string key);
        bool SMove(string sourceKey, string destinationKey, string value);
        string SPop(string key);
        string[] SRandMember(string key, int count = -1);
        int SRemove(string key, params string[] members);
        string[] SUnion(string key, params string[] keys);
        int SUnionStore(string destination, string key, params string[] keys);
        bool HSet(string key, string field, string value);
        int HDelete(string key, params string[] fields);
        bool HExists(string key, string field);
        string HGet(string key, string field);
        string[] HGetAll(string key);
        long HIncrementBy(string key, string field, long delta);
        int HLen(string key);
        string[] HKeys(string key);
        string[] HValues(string key);
        string LIndex(string key, int index);
        int LPush(string key, params string[] values);
        int RPush(string key, params string[] values);
        int NPush(string key, bool head, params string[] values);
        int LInsert(string key, string pivot, string value, bool before = true);
        int LLength(string key);
        string LPop(string key);
        string RPop(string key);
        void LSet(string key, int index, string value);
    }
}