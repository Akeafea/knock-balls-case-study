using KnockBallsCase.Core.Pooling;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Ball
{
    [RequireComponent(typeof(PooledInstance))]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class BallAutoRecycle : MonoBehaviour, IPoolable
    {
        [SerializeField] private float maxLifeTime = 8f;
        [SerializeField] private float minYToRecycle = -3f;

        [Header("Physics Stability")]
        [SerializeField] private bool enforceContinuousCollision = true;
        [SerializeField] private CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        [SerializeField] private RigidbodyInterpolation interpolationMode = RigidbodyInterpolation.Interpolate;

        private PooledInstance _pooledInstance;
        private Rigidbody _rb;
        private float _lifeTimer;

        private void Awake()
        {
            _pooledInstance = GetComponent<PooledInstance>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= maxLifeTime || transform.position.y <= minYToRecycle)
            {
                Recycle();
            }
        }

        public void OnSpawned()
        {
            _lifeTimer = 0f;

            if (_rb != null)
            {
                if (enforceContinuousCollision)
                {
                    _rb.collisionDetectionMode = collisionDetectionMode;
                    _rb.interpolation = interpolationMode;
                }

                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        public void OnDespawned()
        {
            _lifeTimer = 0f;

            if (_rb != null)
            {
                _rb.linearVelocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
        }

        private void Recycle()
        {
            if (_pooledInstance != null)
            {
                _pooledInstance.ReturnToPool();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
