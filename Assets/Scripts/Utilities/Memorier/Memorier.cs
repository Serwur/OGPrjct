using System;
using System.Collections;
using System.Collections.Generic;
using ColdCry.AI;

namespace ColdCry.Utility.Patterns.Memory
{
    public class Memorier : IEnumerable, ICollection
    {
        private LinkedListNode<MemoryPart> current;
        private IMemoriable memoriable;

        public Memorier(IMemoriable memoriable) : this( memoriable, 25, 25 )
        {
        }

        public Memorier(IMemoriable memoriable,int maxUndo, int maxRedo)
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

            IEnumerator<MemoryPart> it = Memory.GetEnumerator();

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
            foreach (MemoryPart memories in Memory) {
                foreach (KeyValuePair<string, object> field in memories) {
                    yield return field;
                }
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
        public MemoryPart CurrentSave { get; private set; }
        private LinkedListNode<MemoryPart> Current
        {
            get => current;
            set {
                current = value;
                CurrentSave = current.Value;
            }
        }
        public LinkedList<MemoryPart> Memory { get; private set; } = new LinkedList<MemoryPart>();
        public IMemoriable Memoriable { get => memoriable; set => memoriable = value; }
        public IMemoriable Memoriable1 { get => memoriable; set => memoriable = value; }
        public bool IsSynchronized => throw new NotImplementedException();
        public object SyncRoot => throw new NotImplementedException();
    }
}
