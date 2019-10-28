using System;
using UnityEngine;

namespace ColdCry.Utility
{
    [Serializable]
    public class Logger
    {
        [SerializeField] private bool logging = true;
        private ILogger logger = Debug.unityLogger;
        private object @object;
        private GameObject gameObject;

        private Logger()
        { }

        public static Logger GetInstance(object @object)
        {
            return new Logger {
                @object = @object
            };
        }

        public static Logger GetInstance(GameObject gameObject)
        {
            return new Logger {
                @object = gameObject,
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
            string message = @object != null ? @object.ToString() + ", " : "";
            message += gameObject != null ? gameObject.name + ": " : "";
            message += text;
            return message;
        }

        public bool Logging { get => logging; set => logging = value; }
    }
}
