using UnityEngine;
using UnityEditor;
namespace ColdCry
{
    namespace Exception
    {
        /// <summary>
        /// Use only when something should never happen in this game
        /// </summary>
        public class MissingTypeException : System.Exception
        {
            public MissingTypeException(string message) : base( message )
            {
            }
        }
    }
}