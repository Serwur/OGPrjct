using System;
using System.Collections;
using System.Collections.Generic;

namespace ColdCry.Utility
{
    public class MultiDictionaryList<TKey, TValue> : MultiDictionary<TKey, TValue, List<TValue>>
    {
        public override TValue Add(TKey key, TValue value)
        {
            if (data.ContainsKey( key )) {
                data[key].Add( value );
            } else {
                data.Add( key, new List<TValue> { value } );
            }
            ValueCount++;
            return value;
        }

        public override TValue[] AddAll(TKey key, TValue[] values)
        {
            if (data.ContainsKey( key )) {
                data[key].AddRange( values );
            } else {
                List<TValue> list = new List<TValue>( values );
                data.Add( key, list );
            }
            ValueCount += values.Length;
            return values;
        }

        public TValue Get(TKey key, int index)
        {
            if (data.TryGetValue( key, out List<TValue> list )) {
                return list[index];
            }
            return default;
        }

        public TValue Get(TKey key, Predicate<TValue> predicate)
        {
            if (data.TryGetValue( key, out List<TValue> list )) {
                return list.Find( predicate );
            }
            return default;
        }

        public TValue Get(TKey key, object obj)
        {
            if (data.TryGetValue( key, out List<TValue> list )) {
                long hash = obj.GetHashCode();
                return list.Find( val => val.GetHashCode() == hash );
            }
            return default;
        }

        public void RemoveAt(TKey key, int index)
        {
            data[key].RemoveAt( index );
        }

        public TValue RandomFromKey(TKey key)
        {
            return Random.FromArray( Get( key ) );
        }
    }

}