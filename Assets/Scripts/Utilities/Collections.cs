using System;
using System.Collections.Generic;
using System.Linq;
using ColdCry.AI;

namespace ColdCry.Utility
{
    public static class Collections
    {
        /// <summary>
        /// Adds all element from given <paramref name="collection"/> to <see cref="LinkedList"/>
        /// </summary>
        /// <param name="collection">Collection to convert</param>
        /// <returns>LinkedList with elements from given collection</returns>
        public static LinkedList<object> ToLinkedList(ICollection<object> collection)
        {
            LinkedList<object> list = new LinkedList<object>();
            foreach (object o in collection) {
                list.AddLast( o );
            }
            return list;
        }

        /// <summary>
        /// Sorts an array using user comparer to compare objects
        /// </summary>
        /// <param name="array">Array to sort</param>
        /// <param name="comparer">Defined comparer to compare objects in array</param>
        public static void Sort(object[] array, IComparer<object> comparer)
        {
            MergeSort( array, 0, array.Length - 1, comparer );
        }

        /// <summary>
        /// Sorts given linked list using custom comparer
        /// </summary>
        /// <param name="linkedList">LinkedList to sort</param>
        /// <param name="comparer">Custom comparer to compare objects in linked list</param>
        public static void Sort(LinkedList<object> linkedList, IComparer<object> comparer)
        {
            object[] array = new object[linkedList.Count];
            MergeSort( array, 0, array.Length - 1, comparer );
            linkedList.Clear();
            foreach (object o in array) {
                linkedList.AddLast( o );
            }
        }

        /// <summary>
        /// Creates <see cref="List"/> of strings using <see cref="ToString"/> method on objects in array
        /// </summary>
        /// <param name="array">Array to convert</param>
        /// <returns>List of strings</returns>
        public static List<string> ToStringList(object[] array)
        {
            string func(object o) => o.ToString();
            return ToStringList( array, func );
        }

        /// <summary>
        /// Creates <see cref="List"/> of <see cref="string"/> using custom function
        /// </summary>
        /// <param name="array">Array to convert</param>
        /// <param name="action">Action that must return string object</param>
        /// <returns>List of strings</returns>
        public static List<string> ToStringList<T>(T[] array, Func<T, string> action)
        {
            List<string> stringList = new List<string>();
            foreach (T o in array) {
                stringList.Add( action( o ) );
            }
            return stringList;
        }

        /// <summary>
        /// Creates an array of <see cref="string"/> from objects using <see cref="ToString"/> method
        /// </summary>
        /// <param name="array">Array to convert</param>
        /// <returns>Array of strings</returns>
        public static string[] ToStringArray(object[] array)
        {
            string[] stringArr = new string[array.Length];
            for (int i = 0; i < array.Length; i++) {
                stringArr[i] = array[i].ToString();
            }
            return stringArr;
        }

        /// <summary>
        /// Creates an array of <see cref="string"/> using custom function
        /// </summary>
        /// <param name="array">Array to convert</param>
        /// <param name="action">Custom function that must return string object</param>
        /// <returns></returns>
        public static string[] ToStringArray(object[] array, Func<object, string> action)
        {
            string[] stringArr = new string[array.Length];
            for (int i = 0; i < array.Length; i++) {
                stringArr[i] = action( array[i] );
            }
            return stringArr;
        }

        public static string[] ToExtendedStringArray(IExtendedString[] array)
        {
            string[] sArr = new string[array.Length];
            for (int i = 0; i < array.Length; i++ ) {
                sArr[i] = array[i].ToExtendedString();
            }
            return sArr;
        }

        /// <summary>
        /// Short type for removing and getting first element from given <see cref="LinkedList{T}"/>
        /// </summary>
        /// <typeparam name="T">Any class type</typeparam>
        /// <param name="linkedList"><see cref="LinkedList{T}"/> to remove and gets first object from it</param>
        /// <returns>First, removed object</returns>
        public static T RemoveFirst<T>(LinkedList<T> linkedList)
        {
            T result = linkedList.First.Value;
            linkedList.RemoveFirst();
            return result;
        }

        /// <summary>
        /// Puts all objects from collection into array
        /// </summary>
        /// <typeparam name="T">Any class</typeparam>
        /// <param name="collection"><see cref="ICollection{T}"/> of objects</param>
        /// <returns>Array of objects from given <see cref="ICollection{T}"/></returns>
        public static T[] ToArray<T>(ICollection<T> collection)
        {
            T[] array = new T[collection.Count];
            int i = 0;
            foreach (T t in collection) {
                array[i] = t;
                i++;
            }
            return array;
        }

        /// <summary>
        /// Sorts an array using merge sort algorithm
        /// </summary>
        /// <param name="array">An array to sort</param>
        /// <param name="start">Start index</param>
        /// <param name="end">End index</param>
        /// <param name="comparer">Comparer to compare objects</param>
        private static void MergeSort(object[] array, int start, int end, IComparer<object> comparer)
        {
            if (start < end) {
                int half = start + ( end - start ) / 2;

                MergeSort( array, start, half, comparer );
                MergeSort( array, half + 1, end, comparer );

                int leftSize = half - start + 1;
                int rightSize = end - half;

                object[] leftTemplate = new object[leftSize];
                for (int i = 0; i < leftSize; i++) {
                    leftTemplate[i] = array[start + i];
                }

                object[] rightTemplate = new object[rightSize];
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

        /// <summary>
        /// Gets enumarator for values of enum
        /// </summary>
        /// <typeparam name="T">Type of enum</typeparam>
        /// <returns>Enumarators values of given enum type</returns>
        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues( typeof( T ) ).Cast<T>();
        }

        /// <summary>
        /// Amount of implemented enums of given type
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns>Amount of enums of given type</returns>
        public static int CountValues<T>()
        {
            return GetValues<T>().Count();
        }

        // To change, need to implement own linked list
        public static LinkedList<T> RemoveFrom<T>(LinkedList<T> linkedList, LinkedListNode<T> from)
        {
            LinkedList<T> result = new LinkedList<T>();
            LinkedListNode<T> current = linkedList.First;
            while (current != null && current != from) {
                result.AddLast( current );
                current = current.Next;
            }
            return result;
        }

        public static LinkedList<T> RemoveTo<T>(LinkedList<T> linkedList, LinkedListNode<T> to)
        {
            LinkedList<T> result = new LinkedList<T>(linkedList);
            LinkedListNode<T> current = linkedList.First;
            while (current != null && current != to) {
                current = current.Next;
                result.RemoveFirst();
            }
            return result;
        }

        public static int CountFrom<T>(LinkedList<T> list, LinkedListNode<T> node)
        {
            throw new NotImplementedException();
        }

        public static int CountTo<T>(LinkedList<T> list, LinkedListNode<T> node)
        {
            throw new NotImplementedException();
        }

    }
}

