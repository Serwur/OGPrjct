using System;
using System.Collections;
using System.Collections.Generic;

namespace ColdCry.Utility
{
    public abstract class MultiDictionary<TKey, TValue, TCollection> where TCollection : ICollection<TValue>
    {
        protected Dictionary<TKey, TCollection> data = new Dictionary<TKey, TCollection>();

        abstract public TValue Add(TKey key, TValue value);
        abstract public TValue[] AddAll(TKey key, TValue[] values);

        /// <summary>
        /// Size of array from given key
        /// </summary>
        /// <param name="key">key of dictionary</param>
        /// <returns>Size of array from given key</returns>
        public int ValuesInKey(TKey key)
        {
            if (data.TryGetValue( key, out TCollection collection )) {
                return collection.Count;
            }
            return -1;
        }

        /// <summary>
        /// Gets all values as array from given key
        /// </summary>
        /// <param name="key">key</param>
        /// <returns>Values as array or <b>null</b> if key doesn't exists in dictionary</returns>
        public TValue[] Get(TKey key)
        {
            if (!data.TryGetValue( key, out TCollection collection ))
                return null;
            TValue[] array = new TValue[collection.Count];
            collection.CopyTo( array, 0 );
            return array;
        }

        /// <summary>
        /// Removes all values from given key and itself
        /// </summary>
        /// <param name="key">key</param>
        /// <returns><b>True</b> if given key exists in dictionary, otherwise <b>false</b></returns>
        public bool RemoveKey(TKey key)
        {
            if (data.ContainsKey( key )) {
                ValueCount -= data[key].Count;
                data.Remove( key );
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes first found value in dictionary
        /// </summary>
        /// <param name="value">value to remove</param>
        /// <returns><b>True</b> if any value has been removed, otherwise <b>false</b></returns>
        public bool RemoveValue(TValue value)
        {
            bool removed = false;
            foreach (TCollection hs in data.Values) {
                if (hs.Remove( value )) {
                    ValueCount--;
                    removed = true;
                }
            }
            return removed;
        }

        /// <summary>
        /// Remove first found value from given key
        /// </summary>
        /// <param name="value">Value to remove</param>
        /// <param name="key">Key</param>
        /// <returns><b>True</b> if given key exists and values has been removed, otherwise <b>false</b></returns>
        public bool RemoveValue(TValue value, TKey key)
        {
            if (!data.ContainsKey( key ))
                return false;
            if (data[key].Remove( value )) {
                ValueCount--;
                return true;
            }
            return false;
        }

        public bool ContainsKey(TKey key)
        {
            return ContainsKey( key );
        }

        public bool ContainsValue(TValue value)
        {
            foreach (TCollection hs in data.Values) {
                if (hs.Contains( value )) {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsValue(TValue value, TKey key)
        {
            if (data.ContainsKey( key )) {
                return data[key].Contains( value );
            }
            return false;
        }

        public void Clear()
        {
            data.Clear();
        }

        public void ClearKey(TKey key)
        {
            data[key].Clear();
        }

        public bool ForEachInKey(TKey key, Action<TValue> action)
        {
            if (data.ContainsKey( key )) {
                foreach (TValue value in data[key]) {
                    action.Invoke( value );
                }
                return true;
            }
            return false;
        }

        public void ForEachValue(Action<TValue> action)
        {
            foreach (TCollection hs in data.Values) {
                foreach (TValue value in hs) {
                    action.Invoke( value );
                }
            }
        }

        public int KeyCount { get => data.Count; }
        public int ValueCount { get; protected set; } = 0;
        public ICollection Keys => data.Keys;
        public ICollection Values
        {
            get {
                LinkedList<TValue> values = new LinkedList<TValue>();
                ForEachValue( (value) => { values.AddLast( value ); } );
                return values;
            }
        }

    }

}