using System;

namespace ColdCry.Utility
{
    public static class Random
    {
        public static T EnumFromArray<T>(T[] objects) where T : struct, IConvertible
        {
            return objects[UnityEngine.Random.Range( 0, objects.Length )];
        }

        public static T[] FewEnumFromArray<T>(T[] objects, int size) where T : struct, IConvertible
        {
            if (size <= 0 || size > objects.Length) {
                throw new System.Exception( "Argument 'size' cannot be 0, less than 0 or greater than array size" );
            }
            if (size >= objects.Length) {
                return objects;
            }

            T[] objs = new T[size];
            bool[] drawn = new bool[objects.Length];
            for (int i = 0; i < objects.Length; i++)
                drawn[i] = new bool();

            int randomized = 0;
            while (randomized != size) {
                int random = UnityEngine.Random.Range( 0, objects.Length );
                if (drawn[random])
                    continue;
                drawn[random] = true;
                objs[randomized] = objects[random];
                randomized++;
            }
            return objs;
        }

        public static T FromArray<T>(T[] objects)
        {
            return objects[UnityEngine.Random.Range( 0, objects.Length )];
        }
    }
}

