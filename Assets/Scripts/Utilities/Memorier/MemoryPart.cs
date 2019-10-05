using System;
using System.Collections;
using System.Collections.Generic;

namespace ColdCry.Utility.Patterns.Memory
{
    public class MemoryPart : IEnumerable<KeyValuePair<string, object>>
    {
        private Dictionary<string, object> values = new Dictionary<string, object>();

        public void Add(string fieldName, object value)
        {
            try {
                values.Add( fieldName, value );
            } catch (ArgumentNullException) {
                throw new MissingFieldException( "Field name cannot be null/empty" );
            } catch (ArgumentException) {
                throw new FieldAccessException( fieldName + " is already defined" );
            }
        }

        public T Get<T>(string fieldName)
        {
            try {
                return (T) values[fieldName];
            } catch (KeyNotFoundException) {
                throw new FieldAccessException( fieldName + " is not defined" );
            }
        }

        public void Set(string fieldName, object value)
        {
            try {
                values[fieldName] = value;
            } catch (ArgumentNullException) {
                throw new MissingFieldException( "Field name cannot be null/empty" );
            } catch (KeyNotFoundException) {
                throw new FieldAccessException( fieldName + " is not defined" );
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public int Count { get => values.Count; }
    }
}
