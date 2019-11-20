using ColdCry.Exception;
using System.Collections.Generic;
using UnityEngine;

namespace ColdCry.Audio
{

    public class AudioManager : MonoBehaviour
    {
        private static AudioManager Instance;
        [SerializeField] private Sound[] soundsSetup;
        private Dictionary<string, Sound> sounds = new Dictionary<string, Sound>();

        protected void Awake()
        {
            if (Instance != null) {
                throw new SingletonException( "Cannot create second object of type " + GetType().Name );
            }
            Instance = this;
            DontDestroyOnLoad( gameObject );

            foreach (Sound sound in Instance.soundsSetup) {
                sound.Source = gameObject.AddComponent<AudioSource>();
                sound.Source.clip = sound.Clip;
                sound.Source.volume = sound.Volume;
                sound.Source.pitch = sound.Pitch;
                Instance.sounds.Add( sound.Name, sound );
            }
        }

        public static void Play(string soundName)
        {
            if (Instance.sounds.TryGetValue( soundName, out Sound sound )) {
                sound.Source.Play();
            } else {
                Debug.LogWarning( "Cannot find sound with name: " + soundName );
            }
        }

    }
}
