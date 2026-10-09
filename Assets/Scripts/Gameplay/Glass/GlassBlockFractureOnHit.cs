using System;
using KnockBallsCase.Core.Pooling;
using KnockBallsCase.Gameplay.Audio;
using KnockBallsCase.Gameplay.Ball;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Glass
{
    [RequireComponent(typeof(Collider))]
    public sealed class GlassBlockFractureOnHit : MonoBehaviour
    {
        public static event Action GlassBroken;

        [Header("References")]
        [SerializeField] private GameObjectPool fracturedGlassPool;
        [SerializeField] private bool useScenePoolProvider = true;

        [Header("Hit Filter")]
        [SerializeField] private string ballTag = "Ball";
        [SerializeField] private float minImpactSpeed = 0.1f;
        [SerializeField] private bool fractureOnGroundCollision = true;
        [SerializeField] private string groundTag = "Ground";

        [Header("Audio")]
        [SerializeField] private AudioClip glassBreakSfx;
        [SerializeField] [Range(0f, 1f)] private float glassBreakSfxVolume = 1f;

        private PooledInstance _pooledInstance;
        private bool _isBroken;
        private bool _poolMissingLogged;

        private void Awake()
        {
            _pooledInstance = GetComponent<PooledInstance>();
        }

        private void OnEnable()
        {
            _isBroken = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isBroken)
            {
                return;
            }

            if (fractureOnGroundCollision && IsGroundCollision(collision))
            {
                Break(collision);
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

            Break(collision);
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
            if (collision.gameObject == null)
            {
                return false;
            }

            if (string.IsNullOrEmpty(groundTag))
            {
                return false;
            }

            return collision.gameObject.CompareTag(groundTag);
        }

        private void Break(Collision collision)
        {
            var pool = ResolveFracturedPool();
            if (pool == null)
            {
                if (!_poolMissingLogged)
                {
                    _poolMissingLogged = true;
                    Debug.LogWarning("Fractured glass pool is not assigned. Assign it directly or add FracturedGlassPoolProvider in scene.", this);
                }

                _isBroken = false;
                return;
            }

            _isBroken = true;
            GlobalSfx2D.Play(glassBreakSfx, glassBreakSfxVolume);
            GlassBroken?.Invoke();

            var fracturedObject = pool.Rent();
            if (fracturedObject != null)
            {
                fracturedObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                fracturedObject.transform.localScale = transform.localScale;

                var fracture = fracturedObject.GetComponent<FracturedGlassBehaviour>();
                if (fracture != null)
                {
                    var contactPoint = collision.contactCount > 0
                        ? collision.GetContact(0).point
                        : transform.position;
                    fracture.StartFracture(contactPoint, collision.relativeVelocity);
                }
            }

            if (_pooledInstance != null)
            {
                _pooledInstance.ReturnToPool();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private GameObjectPool ResolveFracturedPool()
        {
            if (fracturedGlassPool != null)
            {
                return fracturedGlassPool;
            }

            if (!useScenePoolProvider)
            {
                return null;
            }

            return FracturedGlassPoolProvider.Pool;
        }
    }
}
