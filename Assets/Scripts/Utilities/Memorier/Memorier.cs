using System;
using System.Collections;
using System.Collections.Generic;

namespace ColdCry.Utility.Patterns.Memory
{
    public class Memorier<T> : IEnumerable, ICollection
    {
        private LinkedListNode<T> current;
        private IMemoriable<T> memoriable;
        private bool maxRedo = true;
        private bool maxUndo = false;

        public Memorier(IMemoriable<T> memoriable) : this( memoriable, 25, 25 )
        {
        }

        public Memorier(IMemoriable<T> memoriable, int maxUndo, int maxRedo)
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
            if (Current != Memory.First) {
                Memory = Collections.RemoveTo( Memory, Current );
            }
            Memory.AddFirst( memoriable.SaveMemory() );
            Current = Memory.First;
            maxRedo = true;
            maxUndo = false;
        }

        public void Undo()
        {
            if (Current != null && maxUndo == false) {
                maxRedo = false;
                memoriable.LoadMemory( Current.Value );
                if (Current.Next == null) {
                    maxUndo = true;
                } else {
                    Current = Current.Next;
                }
            }
        }

        public void Redo()
        {
            if (Current != null && maxRedo == false) {
                maxUndo = false;
                memoriable.LoadMemory( Current.Value );
                if (Current.Previous == null) {
                    maxRedo = true;
                } else {
                    Current = Current.Previous;
                }
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
