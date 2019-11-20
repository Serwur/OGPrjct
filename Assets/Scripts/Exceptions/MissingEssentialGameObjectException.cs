using System;
using System.Runtime.Serialization;

namespace ColdCry.Exception
{
    [Serializable]
    internal class MissingEssentialGameObjectException : System.Exception
    {
        public MissingEssentialGameObjectException()
        {
        }

        public MissingEssentialGameObjectException(string message) : base( message )
        {
        }

        public MissingEssentialGameObjectException(string message, System.Exception innerException) : base( message, innerException )
        {
        }

        protected MissingEssentialGameObjectException(SerializationInfo info, StreamingContext context) : base( info, context )
        {
        }
    }
}