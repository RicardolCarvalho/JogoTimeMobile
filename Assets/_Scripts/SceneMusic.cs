using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public AudioClip sceneMusic;
    [Range(0f, 1f)] public float volume = 0.7f;

    private AudioSource audioSource;

    void Awake()
    {
        // Unity 2023+ recomendado
        AudioSource[] allAudioSources =
            FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (AudioSource a in allAudioSources)
            a.Stop();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = sceneMusic;
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Som "global" (2D) e volume controlável
        audioSource.spatialBlend = 0f; // 0 = 2D
        audioSource.volume = volume;

        audioSource.Play();
    }
}
