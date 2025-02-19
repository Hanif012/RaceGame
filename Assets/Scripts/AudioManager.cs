using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource; // Background music source
    [SerializeField] private AudioSource sfxSource; // Sound effects source

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Audio Clips")]
    public List<AudioClip> bgmClips;
    public List<AudioClip> sfxClips;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateAudioSourceVolumes();
    }

    // Update volumes based on settings
    private void UpdateAudioSourceVolumes()
    {
        if (bgmSource != null)
            bgmSource.volume = masterVolume * bgmVolume;
        if (sfxSource != null)
            sfxSource.volume = masterVolume * sfxVolume;
    }

    // Play background music
    public void PlayBGM(string clipName, bool loop = true)
    {
        AudioClip clip = bgmClips.Find(c => c.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning($"BGM Clip '{clipName}' not found!");
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    // Play sound effect
    public void PlaySFX(string clipName)
    {
        AudioClip clip = sfxClips.Find(c => c.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning($"SFX Clip '{clipName}' not found!");
            return;
        }

        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    // Stop background music
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
            bgmSource.Stop();
    }

    // Adjust volume dynamically
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }

    [field: SerializeField] public float BGMVolume { get; private set; }
}
