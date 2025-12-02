using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour
{
    public AudioClip[] musicClips; // Assign clips in Inspector
    private List<AudioClip> shuffledClips;
    private AudioSource audioSource;
    private int currentClipIndex = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        ShuffleClips();
        PlayNextClip();
    }

    void Update()
    {
        if (!audioSource.isPlaying && musicClips.Length > 0)
        {
            PlayNextClip();
        }
    }

    void ShuffleClips()
    {
        shuffledClips = new List<AudioClip>(musicClips);
        for (int i = 0; i < shuffledClips.Count; i++)
        {
            int rand = Random.Range(i, shuffledClips.Count);
            AudioClip temp = shuffledClips[i];
            shuffledClips[i] = shuffledClips[rand];
            shuffledClips[rand] = temp;
        }
        currentClipIndex = 0;
    }

    void PlayNextClip()
    {
        if (currentClipIndex >= shuffledClips.Count)
        {
            ShuffleClips();
        }
        audioSource.clip = shuffledClips[currentClipIndex];
        audioSource.Play();
        currentClipIndex++;
    }
}
