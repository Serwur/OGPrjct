using UnityEngine;

[System.Serializable]
public class Sound
{
    [SerializeField] private string name;
    [SerializeField] private AudioClip clip;
    [SerializeField] [Range( 0f, 1f )] private float volume = 1f;
    [SerializeField] [Range( .1f, 3f )] private float pitch = 1f;
    private AudioSource source;

    public string Name { get => name; set => name = value; }
    public AudioClip Clip { get => clip; set => clip = value; }
    public AudioSource Source { get => source; set => source = value; }
    public float Volume { get => volume; set => volume = value; }
    public float Pitch { get => pitch; set => pitch = value; }
}