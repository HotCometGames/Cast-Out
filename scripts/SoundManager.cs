using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class SoundEffectEntry
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0f, 1f)] public float spatialBlend = 0f;
    public float minDistance = 1f;
    public float maxDistance = 20f;
}

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Sound Effects")]
    public SoundEffectEntry[] soundEffects;
    public AudioMixerGroup outputMixerGroup;

    private Dictionary<string, SoundEffectEntry> soundLookup = new Dictionary<string, SoundEffectEntry>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            BuildLookup();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnValidate()
    {
        BuildLookup();
    }

    void BuildLookup()
    {
        soundLookup.Clear();
        if (soundEffects == null)
            return;

        foreach (var entry in soundEffects)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.name) || entry.clip == null)
                continue;

            if (!soundLookup.ContainsKey(entry.name))
                soundLookup.Add(entry.name, entry);
        }
    }

    public static void PlaySound(string soundName, float volume = 1f)
    {
        if (Instance == null)
        {
            Debug.LogWarning($"SoundManager not found. Cannot play sound '{soundName}'.");
            return;
        }

        Instance.Play(soundName, volume, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
    }

    public static void PlaySound(string soundName, Vector3 position, float volume = 1f)
    {
        if (Instance == null)
        {
            Debug.LogWarning($"SoundManager not found. Cannot play sound '{soundName}'.");
            return;
        }

        Instance.Play(soundName, volume, position);
    }

    private void Play(string soundName, float volume, Vector3 position)
    {
        if (!soundLookup.TryGetValue(soundName, out SoundEffectEntry entry))
        {
            Debug.LogWarning($"SoundManager: sound '{soundName}' not found.");
            return;
        }

        if (entry.clip == null)
            return;

        GameObject audioObject = new GameObject($"SFX_{soundName}");
        audioObject.transform.position = position;
        audioObject.transform.parent = transform;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = entry.clip;
        source.volume = Mathf.Clamp01(volume * entry.volume);
        source.spatialBlend = entry.spatialBlend;
        source.minDistance = entry.minDistance;
        source.maxDistance = entry.maxDistance;
        source.outputAudioMixerGroup = outputMixerGroup;
        source.Play();

        Destroy(audioObject, entry.clip.length + 0.1f);
    }
}
