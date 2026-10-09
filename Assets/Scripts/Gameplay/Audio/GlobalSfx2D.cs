using UnityEngine;

namespace KnockBallsCase.Gameplay.Audio
{
    public sealed class GlobalSfx2D : MonoBehaviour
    {
        private const string SoundEnabledPrefKey = "KnockBallsCase.SoundEnabled";

        private static GlobalSfx2D _instance;
        private AudioSource _audioSource;

        public static bool IsSoundEnabled => PlayerPrefs.GetInt(SoundEnabledPrefKey, 1) == 1;

        public static void Play(AudioClip clip, float volume = 1f)
        {
            if (clip == null || !IsSoundEnabled)
            {
                return;
            }

            EnsureInstance();
            if (_instance == null || _instance._audioSource == null)
            {
                return;
            }

            _instance._audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
        }

        public static void SetSoundEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(SoundEnabledPrefKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ToggleSoundEnabled()
        {
            SetSoundEnabled(!IsSoundEnabled);
        }

        private static void EnsureInstance()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = FindAnyObjectByType<GlobalSfx2D>();
            if (_instance == null)
            {
                var go = new GameObject("GlobalSfx2D");
                _instance = go.AddComponent<GlobalSfx2D>();
            }

            _instance.Initialize();
        }

        private void Initialize()
        {
            if (_audioSource == null)
            {
                _audioSource = GetComponent<AudioSource>();
            }

            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }

            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f;
            _audioSource.loop = false;

            if (transform.parent != null)
            {
                transform.SetParent(null);
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}
