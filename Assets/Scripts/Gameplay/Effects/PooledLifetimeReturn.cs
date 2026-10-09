using KnockBallsCase.Core.Pooling;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Effects
{
    [RequireComponent(typeof(PooledInstance))]
    public sealed class PooledLifetimeReturn : MonoBehaviour, IPoolable
    {
        [SerializeField] private float lifeTime = 1.5f;

        private PooledInstance _pooledInstance;
        private float _timer;

        private void Awake()
        {
            _pooledInstance = GetComponent<PooledInstance>();
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= Mathf.Max(0.01f, lifeTime))
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

        public void OnSpawned()
        {
            _timer = 0f;
        }

        public void OnDespawned()
        {
            _timer = 0f;
        }
    }
}
