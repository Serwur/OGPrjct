using UnityEngine;
using System;

namespace ColdCry
{
    namespace Exception
    {
        /// <summary>
        /// Use only when something should never happen in this game
        /// </summary>
        public class SingletonException : System.Exception
        {
            public SingletonException(string message) : base( message )
            {
            }
        }
    }


}
