using System.Collections.Generic;
using UnityEngine;

namespace Core.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Audio Clips")]
        [SerializeField] private List<AudioClip> sfxClips = new List<AudioClip>();
        [SerializeField] private List<AudioClip> musicClips = new List<AudioClip>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Ensure we have AudioSources if not assigned in Inspector
                if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
                if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

                musicSource.loop = true;
                musicSource.playOnAwake = false;
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Plays a sound effect from the sfxClips list by index.
        /// </summary>
        public void PlaySFX(int index, float volume = 1f)
        {
            if (index < 0 || index >= sfxClips.Count)
            {
                Debug.LogWarning($"[AudioManager] SFX index {index} out of bounds.");
                return;
            }
            PlaySFX(sfxClips[index], volume);
        }

        /// <summary>
        /// Plays a sound effect once. Supports overlapping sounds.
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Plays a background music track from the musicClips list by index.
        /// </summary>
        public void PlayMusic(int index, float volume = 1f)
        {
            if (index < 0 || index >= musicClips.Count)
            {
                Debug.LogWarning($"[AudioManager] Music index {index} out of bounds.");
                return;
            }
            PlayMusic(musicClips[index], volume);
        }

        /// <summary>
        /// Plays a background music track (looping).
        /// </summary>
        public void PlayMusic(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.volume = volume;
            musicSource.Play();
        }

        /// <summary>
        /// Stops the background music.
        /// </summary>
        public void StopMusic()
        {
            musicSource.Stop();
        }

        /// <summary>
        /// Sets the music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicSource.volume = volume;
        }

        /// <summary>
        /// Sets the SFX volume.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxSource.volume = volume;
        }
    }
}
