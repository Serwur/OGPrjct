using UnityEngine;

namespace DoubleMMPrjc
{
    namespace Utility
    {

        public class Math
        {
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

            public static Vector2 Middle(Vector2 a, Vector2 b)
            {
                return new Vector2( a.x + b.x, a.y + b.y ) / 2f;
            }
        }

    }
}

