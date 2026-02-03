using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Settings.Sound
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] List<AudioClip> clips = new List<AudioClip>();

        [SerializeField] List<AudioClip> effects = new List<AudioClip>();

        [SerializeField] int clipIndex = 0;
        [SerializeField] float delay = 1.5f;

        [SerializeField] float currentTime = 0;
        [SerializeField] float endTime;
        AudioSource source;
        AudioListener listener;

        bool playing = false;
        float effectVolume;

        static MusicPlayer instance;

        IEnumerator playNext;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        static void ClearInstance()
        {
            instance = null;
        }

        private void Awake()
        {
            if(instance != null)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            
            source = GetComponent<AudioSource>();
            listener = GetComponent<AudioListener>();

            clipIndex = 0;
        }

        [ContextMenu("Next")]
        public void Next()
        {
            clipIndex++;
            Play();
        }

        [ContextMenu("Prev")]
        public void Prev()
        {
            clipIndex--;
            Play();
        }

        public void Select(int index)
        {
            clipIndex = System.Math.Clamp(index, 0, clips.Count - 1);
            Play();
        }

        public void Toggle()
        {
            playing = !playing;
            if (playing)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }

        [ContextMenu("Unpause")]
        void Unpause()
        {
            source.UnPause();
            playNext = PlayNextSong();
            StartCoroutine(playNext);
        }

        [ContextMenu("Pause")]
        void Pause()
        {
            source.Pause();
            // Change to stop all
            StopCoroutine(playNext);
            playNext = null;
        }

        #region Music Looping
        /// <summary>
        /// Resets current time, and the time coroutine. 
        /// Plays the current clip at <see cref="clipIndex"/>.
        /// </summary>
        void Play()
        {
            currentTime = 0;
            if(playNext != null)
            {
                StopCoroutine(playNext);
                playNext = null;
            }

            playing = true;

            // wrap index
            if (clipIndex < 0)
                clipIndex = clips.Count - 1;
            else if (clipIndex >= clips.Count)
                clipIndex = 0;

            // select clip and play it
            AudioClip clip = clips[clipIndex];
            source.Stop();
            source.clip = clip;
            source.Play();

            playNext = PlayNextSong();
            StartCoroutine(playNext);
        }

        /// <summary>
        /// Waits until the end of the song (add <see cref="delay"/>), then plays the next clip.
        /// </summary>
        /// <returns></returns>
        IEnumerator PlayNextSong()
        {
            endTime = source.clip.length + delay;
            while (currentTime < endTime)
            {
                yield return new();
                currentTime += Time.deltaTime;
            }
            clipIndex++;
            Play();
        }
        #endregion

        #region Volume Control
        public void SetVolume(bool _mute, int _masterVolume, int _musicVolume, int _effectVolume)
        {
            source.mute = _mute;

            float masterVolume = _masterVolume / 100f;
            effectVolume = (_effectVolume * masterVolume) / 100f;
            source.volume = (_musicVolume * masterVolume) / 100f;
            
            if (enabled == false)
            {
                enabled = true;

                source.enabled = true;
                listener.enabled = true;

                Play();
            }
        }
        #endregion

        #region Sound Effects
        public static void PlaySoundEffect(AudioClip clip)
        {
            instance.PlaySoundEff(clip);
        }

        void PlaySoundEff(AudioClip clip)
        {
            source.PlayOneShot(clip, effectVolume);
        }
        #endregion
    }
}
