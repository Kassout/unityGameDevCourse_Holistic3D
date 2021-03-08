using UnityEngine;

public class RandomSound : MonoBehaviour
{
    public AudioSource sound;
    public bool sound3D = true;
    public float firstPlay;
    public float randomMin;
    public float randomMax;

    // Start is called before the first frame update
    void Start()
    {
        Invoke("PlaySound", firstPlay);
    }

    void PlaySound()
    {
        GameObject newSound = new GameObject();
        AudioSource newAS = newSound.AddComponent<AudioSource>();
        newAS.clip = sound.clip;
        if (sound3D)
        {
            newAS.spatialBlend = 1;
            newAS.maxDistance = sound.maxDistance;
            newSound.transform.SetParent(transform);
            newSound.transform.localPosition = Vector3.zero;
        }
        newAS.Play();

        Invoke("PlaySound", Random.Range(randomMin, randomMax));
        Destroy(newSound, sound.clip.length);
    }
}
