using System.Collections.Generic;

namespace DoubleMMPrjc
{
    namespace Utilities
    {
        public sealed class Utility
        {
            private Utility()
            { }

            public static LinkedList<T> ToLinkedList<T>(ICollection<T> collection)
            {
                LinkedList<T> list = new LinkedList<T>();
                foreach (T t in collection) {
                    list.AddLast( t );
                }
                return list;
            }

            public static void Sort<T>(T[] array, IComparer<T> comparer)
            {
                MergeSort( array, 0, array.Length - 1, comparer );
            }

            private static void MergeSort<T>(T[] array, int start, int end, IComparer<T> comparer)
            {
                if (start < end) {
                    int half = start + ( end - start ) / 2;

                    MergeSort( array, start, half, comparer );
                    MergeSort( array, half + 1, end, comparer );

                    int leftSize = half - start + 1;
                    int rightSize = end - half;

                    T[] leftTemplate = new T[leftSize];
                    for (int i = 0; i < leftSize; i++) {
                        leftTemplate[i] = array[start + i];
                    }

                    T[] rightTemplate = new T[rightSize];
                    for (int i = 0; i < rightSize; i++) {
                        rightTemplate[i] = array[half + i + 1];
                    }

                    int lT = 0;
                    int rT = 0;
                    int listIndex = start;

                    while (lT < leftSize && rT < rightSize) {
                        if (comparer.Compare( leftTemplate[lT], rightTemplate[rT] ) > 0) {
                            array[listIndex++] = leftTemplate[lT++];
                        } else {
                            array[listIndex++] = rightTemplate[rT++];
                        }
                        listIndex++;
                    }

                    while (lT < leftSize) {
                        array[listIndex++] = leftTemplate[lT++];
                    }
                    while (rT < rightSize) {
                        array[listIndex++] = rightTemplate[rT++];
                    }
                }
            }
        }

    }
}

