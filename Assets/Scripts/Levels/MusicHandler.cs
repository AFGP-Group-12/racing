using System;
using UnityEngine;
using UnityEngine.Audio;

// Global music manager. Lives across scenes and plays requested tracks.
public class MusicHandler : MonoBehaviour
{
    public static MusicHandler Instance { get; private set; }

    [Header("Music Settings")]
    [Range(0f, 1f)] public float volume = 0.8f;
    [Range(0f, 1f)] public float spatialBlend3D = 0f; // music is typically 2D

    [Header("Routing")]
    [Tooltip("Assign the AudioMixerGroup for Music.")]
    public AudioMixerGroup musicGroup;

    private AudioSource _music;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure there is an AudioSource configured for global music playback
        _music = GetComponent<AudioSource>();
        if (_music == null)
            _music = gameObject.AddComponent<AudioSource>();

        _music.playOnAwake = false;
        _music.loop = true;
        _music.spatialBlend = spatialBlend3D;
        _music.volume = volume;
        if (musicGroup)
            _music.outputAudioMixerGroup = musicGroup;
    }

    // Call this from anywhere to request a new music track by Resources path.
    public void Play(string resourcesPath)
    {
        if (string.IsNullOrEmpty(resourcesPath)) return;

        var clip = Resources.Load<AudioClip>(resourcesPath);
        if (!clip)
        {
            Debug.LogWarning($"MusicHandler: Could not load clip at '{resourcesPath}'. Ensure it is under a Resources folder.");
            return;
        }

        // Stop if playing something else, then play the requested clip
        if (_music.isPlaying)
            _music.Stop();

        // Route to Music mixer group before playing (if set in Awake)
        _music.clip = clip;
        _music.volume = volume;
        _music.spatialBlend = spatialBlend3D;
        if (clip.loadState == AudioDataLoadState.Unloaded)
            clip.LoadAudioData();
        _music.Play();
    }
}