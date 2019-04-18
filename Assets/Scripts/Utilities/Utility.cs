using System;
using System.Collections.Generic;

namespace DoubleMMPrjc
{
    namespace Utilities
    {
        public sealed class Utility
        {
            private Utility()
            { }

            /// <summary>
            /// Rounds the value to number of digits given by <b>precision</b>
            /// </summary>
            /// <param name="value">Value to round</param>
            /// <param name="precision">Number of digits after comma</param>
            /// <returns>Rounded value</returns>
            public static float Round(float value, int precision = 0)
            {
                int multiplay = 1;
                for (int i = 0; i < precision; i++)
                    multiplay *= 10;
                int rounded = (int) ( value * multiplay );
                return (float) rounded / multiplay;
            }

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
            /// Sorts the array using user comparer to compare objects
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
            public static void Sort(LinkedList<object> linkedList, IComparer<object> comparer) {
                object[] array = new object[linkedList.Count];
                MergeSort(array, 0, array.Length -1, comparer);
                linkedList.Clear();
                foreach ( object o in array ) {
                    linkedList.AddLast(o);
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
            /// Creates <see cref="List"/> of strings using ToExtendedString method
            /// </summary>
            /// <param name="array">Array of objects implementing IExtendedString interface</param>
            /// <returns>List of strings</returns>
            public static List<string> ToExtendedStringList(IExtendedString[] array)
            {
                string func(IExtendedString o) => o.ToExtendedString();
                return ToStringList( array, func );
            }

            /// <summary>
            /// Creates <see cref="List"/> of strings using custom function
            /// </summary>
            /// <param name="array">Array to convert</param>
            /// <param name="action">Action that must return string object</param>
            /// <returns>List of strings</returns>
            public static List<string> ToStringList(object[] array, Func<object, string> action)
            {
                List<string> stringList = new List<string>();
                foreach (object o in array) {
                    stringList.Add( action( o ) );
                }
                return stringList;
            }

            /// <summary>
            /// Creates an array of strings
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
            /// Creates an array of strings using interface IExtendedString
            /// </summary>
            /// <param name="array">Array of objects implementing interface IExtendedString</param>
            /// <returns>Array of strings</returns>
            public static string[] ToExtendedStringArray(IExtendedString[] array)
            {
                string[] stringArr = new string[array.Length];
                for (int i = 0; i < array.Length; i++) {
                    stringArr[i] = array[i].ToExtendedString();
                }
                return stringArr;
            }

            public static string[] ToStringArray(object[] array, Func<object, string> action)
            {
                string[] stringArr = new string[array.Length];
                for (int i = 0; i < array.Length; i++) {
                    stringArr[i] = action( array[i] );
                }
                return stringArr;
            }

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

        }

    }
}

