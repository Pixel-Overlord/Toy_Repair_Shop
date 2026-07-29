using ToyRepairShop.Core;
using UnityEngine;

namespace ToyRepairShop.Managers
{
    /// <summary>
    /// Owns the game's two audio channels (music and one-shot SFX) so no
    /// other code creates or searches for AudioSources directly.
    /// </summary>
    public sealed class AudioManager : MonoSingleton<AudioManager>
    {
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        protected override void Initialize()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
            }

            if (_sfxSource == null)
            {
                _sfxSource = gameObject.AddComponent<AudioSource>();
            }

            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _sfxSource.playOnAwake = false;
        }

        /// <summary>Plays a music clip on the music channel, replacing whatever is currently playing.</summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null)
            {
                return;
            }

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        /// <summary>Stops the music channel.</summary>
        public void StopMusic()
        {
            _musicSource.Stop();
        }

        /// <summary>Plays a one-shot sound effect without interrupting other SFX.</summary>
        public void PlaySFX(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            _sfxSource.PlayOneShot(clip);
        }

        /// <summary>Sets the music channel volume (0-1).</summary>
        public void SetMusicVolume(float volume)
        {
            _musicSource.volume = Mathf.Clamp01(volume);
        }

        /// <summary>Sets the SFX channel volume (0-1).</summary>
        public void SetSFXVolume(float volume)
        {
            _sfxSource.volume = Mathf.Clamp01(volume);
        }
    }
}
