
namespace ColdCry
{
    namespace Exception
    {
        /// <summary>
        /// Use only when something should never happen in this game
        /// </summary>
        public class TranslationException : System.Exception
        {
            public TranslationException(string message) : base( message )
            {
            }
        }
    }
}
