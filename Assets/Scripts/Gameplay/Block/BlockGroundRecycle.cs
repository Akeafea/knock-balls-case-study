using System;
using System.Collections;
using KnockBallsCase.Core.Pooling;
using KnockBallsCase.Gameplay.Audio;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Block
{
    [RequireComponent(typeof(PooledInstance))]
    public sealed class BlockGroundRecycle : MonoBehaviour, IPoolable
    {
        public static event Action BlockDestroyed;

        [SerializeField] private string groundTag = "Ground";
        [SerializeField] private float shrinkDuration = 0.25f;

        [Header("Audio")]
        [SerializeField] private AudioClip woodHitSfx;
        [SerializeField] [Range(0f, 1f)] private float woodHitSfxVolume = 1f;
        [SerializeField] private float woodHitSfxMinInterval = 0.16f;
        [SerializeField] private float woodHitMinImpactSpeed = 1.5f;

        private PooledInstance _pooledInstance;
        private Vector3 _initialScale;
        private Coroutine _shrinkRoutine;
        private bool _isRecycling;
        private float _lastWoodHitSfxTime = -999f;

        private void Awake()
        {
            _pooledInstance = GetComponent<PooledInstance>();
            _initialScale = transform.localScale;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isRecycling)
            {
                return;
            }

            if (collision.relativeVelocity.magnitude >= woodHitMinImpactSpeed)
            {
                PlayWoodHitSfx();
            }

            if (!collision.gameObject.CompareTag(groundTag))
            {
                return;
            }

            BlockDestroyed?.Invoke();
            _shrinkRoutine = StartCoroutine(ShrinkAndRecycle());
        }

        public void OnSpawned()
        {
            _isRecycling = false;
            transform.localScale = _initialScale;
        }

        public void OnDespawned()
        {
            _isRecycling = false;
            transform.localScale = _initialScale;

            if (_shrinkRoutine != null)
            {
                StopCoroutine(_shrinkRoutine);
                _shrinkRoutine = null;
            }
        }

        private IEnumerator ShrinkAndRecycle()
        {
            _isRecycling = true;

            var elapsed = 0f;
            while (elapsed < shrinkDuration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / shrinkDuration);
                transform.localScale = Vector3.Lerp(_initialScale, Vector3.zero, t);
                yield return null;
            }

            transform.localScale = _initialScale;

            if (_pooledInstance != null)
            {
                _pooledInstance.ReturnToPool();
            }
            else
            {
                gameObject.SetActive(false);
            }

            _isRecycling = false;
            _shrinkRoutine = null;
        }

        private void PlayWoodHitSfx()
        {
            if (woodHitSfx == null)
            {
                return;
            }

            if (Time.time - _lastWoodHitSfxTime < Mathf.Max(0f, woodHitSfxMinInterval))
            {
                return;
            }

            _lastWoodHitSfxTime = Time.time;
            GlobalSfx2D.Play(woodHitSfx, woodHitSfxVolume);
        }
    }
}
