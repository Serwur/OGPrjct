using System;
using UnityEngine;

namespace ColdCry.Utility
{
    [Serializable]
    public class Logger
    {
        [SerializeField] private bool logging = true;
        private ILogger logger = Debug.unityLogger;
        private Type type;
        private GameObject gameObject;

        private Logger()
        { }

        public static Logger GetInstance(Type type)
        {
            return new Logger {
                type = type
            };
        }

        public static Logger GetInstance(GameObject gameObject)
        {
            return new Logger {
                type = gameObject.GetType(),
                gameObject = gameObject
            };
        }

        public void Log(object text)
        {
            Debug.Log( Format( text.ToString() ) );
        }

        public void Warn(object text)
        {
            Debug.LogWarning( Format( text.ToString() ) );
        }

        public void Err(object text)
        {
            Debug.LogError( Format( text.ToString() ) );
        }

        private string Format(string text)
        {
            string message = type != null ? type.ToString() + ", " : "";
            message += gameObject != null ? gameObject.name + ": " : "";
            message += text;
            return message;
        }

        public bool Logging { get => logging; set => logging = value; }
    }
}
