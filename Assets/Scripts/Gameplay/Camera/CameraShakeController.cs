using DG.Tweening;
using UnityEngine;

namespace KnockBallsCase.Gameplay.CameraFx
{
    public sealed class CameraShakeController : MonoBehaviour
    {
        [System.Serializable]
        private sealed class ShakePreset
        {
            public float duration = 0.2f;
            public Vector3 strength = new Vector3(0.08f, 0.08f, 0f);
            public int vibrato = 20;
            public float randomness = 70f;
            public bool snapping;
            public bool fadeOut = true;
        }

        [Header("References")]
        [SerializeField] private Transform shakeTarget;
        [SerializeField] private bool useUnscaledTime;

        [Header("Presets")]
        [SerializeField] private ShakePreset cannonFireShake = new ShakePreset
        {
            duration = 0.12f,
            strength = new Vector3(0.05f, 0.05f, 0f),
            vibrato = 16,
            randomness = 80f,
            snapping = false,
            fadeOut = true
        };

        [SerializeField] private ShakePreset explosionShake = new ShakePreset
        {
            duration = 0.3f,
            strength = new Vector3(0.18f, 0.18f, 0f),
            vibrato = 24,
            randomness = 90f,
            snapping = false,
            fadeOut = true
        };

        private Tween _activeTween;
        private Vector3 _cachedLocalPosition;

        private void Awake()
        {
            ResolveTargetIfNeeded();
            CacheCurrentLocalPosition();
        }

        private void OnDisable()
        {
            KillActiveShake();
        }

        public void PlayCannonFireShake()
        {
            PlayShake(cannonFireShake, 1f);
        }

        public void PlayExplosionShake(float strengthMultiplier = 1f)
        {
            PlayShake(explosionShake, Mathf.Max(0f, strengthMultiplier));
        }

        private void PlayShake(ShakePreset preset, float strengthMultiplier)
        {
            if (preset == null)
            {
                return;
            }

            ResolveTargetIfNeeded();
            if (shakeTarget == null)
            {
                return;
            }

            CacheCurrentLocalPosition();
            KillActiveShake();

            var duration = Mathf.Max(0.01f, preset.duration);
            var strength = preset.strength * Mathf.Max(0f, strengthMultiplier);

            _activeTween = shakeTarget.DOShakePosition(
                    duration,
                    strength,
                    Mathf.Max(1, preset.vibrato),
                    Mathf.Max(0f, preset.randomness),
                    preset.snapping,
                    preset.fadeOut)
                .SetUpdate(useUnscaledTime)
                .OnComplete(RestoreLocalPosition)
                .OnKill(RestoreLocalPosition);
        }

        private void ResolveTargetIfNeeded()
        {
            if (shakeTarget != null)
            {
                return;
            }

            var mainCam = Camera.main;
            if (mainCam != null)
            {
                shakeTarget = mainCam.transform;
            }
        }

        private void CacheCurrentLocalPosition()
        {
            if (shakeTarget == null)
            {
                return;
            }

            _cachedLocalPosition = shakeTarget.localPosition;
        }

        private void RestoreLocalPosition()
        {
            if (shakeTarget == null)
            {
                return;
            }

            shakeTarget.localPosition = _cachedLocalPosition;
        }

        private void KillActiveShake()
        {
            if (_activeTween == null)
            {
                return;
            }

            if (_activeTween.IsActive())
            {
                _activeTween.Kill(false);
            }

            _activeTween = null;
        }
    }
}
