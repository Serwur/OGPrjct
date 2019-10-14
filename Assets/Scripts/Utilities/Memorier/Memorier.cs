using System;
using System.Collections;
using System.Collections.Generic;
using ColdCry.AI;

namespace ColdCry.Utility.Patterns.Memory
{
    public class Memorier<T> : IEnumerable, ICollection
    {
        private LinkedListNode<T> current;
        private IMemoriable<T> memoriable;

        public Memorier(IMemoriable<T> memoriable) : this( memoriable, 25, 25 )
        {
        }

        public Memorier(IMemoriable<T> memoriable,int maxUndo, int maxRedo)
        {
            this.memoriable = memoriable;
            MaxUndo = maxUndo;
            MaxRedo = maxRedo;
        }

        public void CopyTo(Array array, int index)
        {
            if (index < 0 || index > Count - 1) {
                throw new IndexOutOfRangeException();
            }

            int size = Count - index;
            int counter = 0;

            IEnumerator<T> it = Memory.GetEnumerator();

            while (counter != index) {
                it.MoveNext();
            }
            array = new Array[size];
            array.SetValue( it.Current, 0 );
            counter = 1;

            while (it.MoveNext() && counter < size) {
                array.SetValue( it.Current, counter );
            }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (T memories in Memory) {
                yield return memories;
            }
        }

        public void Save()
        {
            if (Current != null) {
                Memory = Collections.RemoveFrom( Memory, Current );
            }
            Memory.AddLast( memoriable.SaveMemory() );
            Current = Memory.Last;
        }

        public void Undo()
        {
            if (Current != null && Current.Previous != null) {
                Current = Current.Previous;
                memoriable.LoadMemory( Current.Value );
            }
        }

        public void Redo()
        {
            if (Current != null && Current.Next != null) {
                Current = Current.Next;
                memoriable.LoadMemory( Current.Value );
            }
        }

        public int Count => Memory.Count;
        public int CountUndo { get { return Current == null ? 0 : Collections.CountTo( Memory, Current ); } }
        public int CountRedo { get { return Current == null ? 0 : Collections.CountFrom( Memory, Current ); ; } }
        public int MaxUndo { get; private set; }
        public int MaxRedo { get; private set; }
        public T CurrentSave { get; private set; }
        private LinkedListNode<T> Current
        {
            get => current;
            set {
                current = value;
                CurrentSave = current.Value;
            }
        }
        public LinkedList<T> Memory { get; private set; } = new LinkedList<T>();
        public IMemoriable<T> Memoriable { get => memoriable; set => memoriable = value; }
        public bool IsSynchronized => throw new NotImplementedException();
        public object SyncRoot => throw new NotImplementedException();
    }
}
