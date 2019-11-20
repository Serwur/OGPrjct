using ColdCry.Exception;
using UnityEngine;

namespace ColdCry.Utility
{
    public static class Math
    {

        public static float Distance(float a, float b)
        {
            return Mathf.Abs( a - b );
        }

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

        /// <summary>
        /// Checks if given <b>value</b> is greater than second argument <b>a</b> and third argument <b>b</b>
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Between(float value, float a, float b)
        {
            return ( value >= a && value <= b );
        }

        /// <summary>
        /// Checks if given <b>value</b> is near <b>x</b> value by given <b>slip</b>
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="x">Second value to check if first is near</param>
        /// <param name="slip">Slip</param>
        /// <returns><b>True</b> if first argument is near second including slip, otherwise <b>false</b></returns>
        public static bool IsNear(float value, float x, float slip)
        {
            return Between( value, x - slip, x + slip );
        }

        public static bool IsNearNo0(float value, float x, float slip)
        {
            float first = x - slip;
            float second = x + slip;
            return Between( value, first < 0 ? 0 : first, second < 0 ? 0 : second );
        }

        public static System.Numerics.BigInteger Pow10(System.Numerics.BigInteger value, int times)
        {
            System.Numerics.BigInteger result = value;
            if (times > 0) {
                for (var i = 0; i < times; i++) {
                    result *= 10;
                }
            } else {
                for (var i = 0; i < times; i++) {
                    result /= 10;
                }
            }
            return result;
        }

    }


}

