using System;
using System.Runtime.Serialization;

namespace ColdCry.Exception
{
    [Serializable]
    internal class MissingEssentialGameObject : System.Exception
    {
        public MissingEssentialGameObject()
        {
        }

        public MissingEssentialGameObject(string message) : base( message )
        {
        }

        public MissingEssentialGameObject(string message, System.Exception innerException) : base( message, innerException )
        {
        }

        protected MissingEssentialGameObject(SerializationInfo info, StreamingContext context) : base( info, context )
        {
        }
    }
}