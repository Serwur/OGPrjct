namespace ColdCry.Utility
{
    public static class Objects
    {

        public static bool IsTypeOf<T>(object o)
        {
            return typeof( T ) == o.GetType();
        }

    }
}