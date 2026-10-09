using System;
using System.Collections;
using KnockBallsCase.Core.Pooling;
using KnockBallsCase.Gameplay.Audio;
using KnockBallsCase.Gameplay.Ball;
using KnockBallsCase.Gameplay.CameraFx;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Explosive
{
    [RequireComponent(typeof(Collider))]
    public sealed class ExplosiveBarrelBlock : MonoBehaviour
    {
        public static event Action<int> Exploded;

        [Header("Hit Filter")]
        [SerializeField] private string ballTag = "Ball";
        [SerializeField] private float minImpactSpeed = 0.1f;

        [Header("Score")]
        [SerializeField] private int scoreOnExplosion = 5;
        [SerializeField] private int scoreOnGroundRecycle = 1;

        [Header("Ground Recycle")]
        [SerializeField] private bool recycleOnGroundCollision = true;
        [SerializeField] private string groundTag = "Ground";
        [SerializeField] private float groundShrinkDuration = 0.25f;

        [Header("Explosion Physics")]
        [SerializeField] private float explosionRadius = 4f;
        [SerializeField] private float maxPushForce = 14f;
        [SerializeField] private float upwardForce = 0.15f;
        [SerializeField] private LayerMask affectedLayers = ~0;

        [Header("Explosion Effect")]
        [SerializeField] private GameObjectPool explosionEffectPool;
        [SerializeField] private bool useSceneEffectPoolProvider = true;
        [SerializeField] private CameraShakeController cameraShakeController;

        [Header("Audio")]
        [SerializeField] private AudioClip explosionSfx;
        [SerializeField] [Range(0f, 1f)] private float explosionSfxVolume = 1f;

        private PooledInstance _pooledInstance;
        private bool _isExploded;
        private bool _effectPoolMissingLogged;
        private Vector3 _initialScale;
        private Coroutine _groundRecycleRoutine;

        private void Awake()
        {
            _pooledInstance = GetComponent<PooledInstance>();
            _initialScale = transform.localScale;

            if (cameraShakeController == null)
            {
                cameraShakeController = FindAnyObjectByType<CameraShakeController>();
            }
        }

        private void OnEnable()
        {
            _isExploded = false;
            transform.localScale = _initialScale;

            if (_groundRecycleRoutine != null)
            {
                StopCoroutine(_groundRecycleRoutine);
                _groundRecycleRoutine = null;
            }
        }

        private void OnDisable()
        {
            if (_groundRecycleRoutine != null)
            {
                StopCoroutine(_groundRecycleRoutine);
                _groundRecycleRoutine = null;
            }

            transform.localScale = _initialScale;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isExploded)
            {
                return;
            }

            if (recycleOnGroundCollision && IsGroundCollision(collision))
            {
                _groundRecycleRoutine = StartCoroutine(ShrinkAndRecycleOnGround());
                return;
            }

            if (collision.relativeVelocity.magnitude < minImpactSpeed)
            {
                return;
            }

            if (!IsBallCollision(collision))
            {
                return;
            }

            Explode();
        }

        private bool IsBallCollision(Collision collision)
        {
            var other = collision.gameObject;
            if (!string.IsNullOrEmpty(ballTag) && other.CompareTag(ballTag))
            {
                return true;
            }

            return other.GetComponent<BallAutoRecycle>() != null;
        }

        private bool IsGroundCollision(Collision collision)
        {
            if (collision.gameObject == null || string.IsNullOrEmpty(groundTag))
            {
                return false;
            }

            return collision.gameObject.CompareTag(groundTag);
        }

        private void Explode()
        {
            _isExploded = true;
            if (cameraShakeController != null)
            {
                cameraShakeController.PlayExplosionShake();
            }

            GlobalSfx2D.Play(explosionSfx, explosionSfxVolume);

            SpawnExplosionEffect();
            ApplyExplosionForce();

            Exploded?.Invoke(Mathf.Max(0, scoreOnExplosion));

            if (_pooledInstance != null)
            {
                _pooledInstance.ReturnToPool();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private IEnumerator ShrinkAndRecycleOnGround()
        {
            _isExploded = true;
            Exploded?.Invoke(Mathf.Max(0, scoreOnGroundRecycle));

            var duration = Mathf.Max(0.01f, groundShrinkDuration);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.Lerp(_initialScale, Vector3.zero, t);
                yield return null;
            }

            transform.localScale = _initialScale;
            _groundRecycleRoutine = null;

            if (_pooledInstance != null)
            {
                _pooledInstance.ReturnToPool();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void SpawnExplosionEffect()
        {
            var pool = ResolveEffectPool();
            if (pool == null)
            {
                if (!_effectPoolMissingLogged)
                {
                    _effectPoolMissingLogged = true;
                    Debug.LogWarning("Explosion effect pool is not assigned. Assign it directly or add ExplosionEffectPoolProvider in scene.", this);
                }

                return;
            }

            var effect = pool.Rent();
            if (effect == null)
            {
                return;
            }

            effect.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
        }

        private void ApplyExplosionForce()
        {
            var center = transform.position;
            var radius = Mathf.Max(0.1f, explosionRadius);
            var colliders = Physics.OverlapSphere(center, radius, affectedLayers, QueryTriggerInteraction.Ignore);

            for (var i = 0; i < colliders.Length; i++)
            {
                var rb = colliders[i].attachedRigidbody;
                if (rb == null)
                {
                    continue;
                }

                if (rb.gameObject == gameObject)
                {
                    continue;
                }

                var closest = colliders[i].ClosestPoint(center);
                var direction = closest - center;
                var distance = direction.magnitude;

                if (distance <= 0.0001f)
                {
                    direction = UnityEngine.Random.onUnitSphere;
                    distance = 0.01f;
                }
                else
                {
                    direction /= distance;
                }

                var normalized = Mathf.Clamp01(1f - (distance / radius));
                var force = maxPushForce * normalized;

                var finalDirection = new Vector3(direction.x, direction.y + upwardForce, direction.z).normalized;
                rb.AddForce(finalDirection * force, ForceMode.Impulse);
            }
        }

        private GameObjectPool ResolveEffectPool()
        {
            if (explosionEffectPool != null)
            {
                return explosionEffectPool;
            }

            if (!useSceneEffectPoolProvider)
            {
                return null;
            }

            return ExplosionEffectPoolProvider.Pool;
        }
    }
}
