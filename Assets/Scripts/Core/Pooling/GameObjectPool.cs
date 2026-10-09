using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnockBallsCase.Core.Pooling
{
    public sealed class GameObjectPool : MonoBehaviour, IObjectPool
    {
        [Header("Pool Setup")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;
        [SerializeField] private bool expandable = true;
        [SerializeField] private int hardLimit = 0;

        private readonly Queue<GameObject> _available = new Queue<GameObject>();
        private readonly HashSet<GameObject> _allInstances = new HashSet<GameObject>();

        public int ActiveCount { get; private set; }
        public int TotalCount => _allInstances.Count;

        public event Action<int> ActiveCountChanged;

        private void Awake()
        {
            if (prefab == null)
            {
                Debug.LogError($"{nameof(GameObjectPool)} on {name} has no prefab assigned.");
                return;
            }

            for (var i = 0; i < initialSize; i++)
            {
                var instance = CreateInstance();
                if (instance != null)
                {
                    _available.Enqueue(instance);
                }
            }
        }

        public GameObject Rent()
        {
            if (_available.Count == 0)
            {
                if (!expandable)
                {
                    return null;
                }

                if (hardLimit > 0 && _allInstances.Count >= hardLimit)
                {
                    return null;
                }

                var newInstance = CreateInstance();
                if (newInstance != null)
                {
                    _available.Enqueue(newInstance);
                }
            }

            if (_available.Count == 0)
            {
                return null;
            }

            var instance = _available.Dequeue();
            if (instance == null)
            {
                return null;
            }

            instance.transform.SetParent(null);
            ResetRigidbodyState(instance);
            instance.SetActive(true);
            NotifyPoolablesSpawned(instance);

            ActiveCount++;
            ActiveCountChanged?.Invoke(ActiveCount);
            return instance;
        }

        public void Return(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (!_allInstances.Contains(instance))
            {
                instance.SetActive(false);
                return;
            }

            NotifyPoolablesDespawned(instance);
            ResetRigidbodyState(instance);
            instance.transform.SetParent(transform);
            instance.SetActive(false);
            _available.Enqueue(instance);

            ActiveCount = Mathf.Max(0, ActiveCount - 1);
            ActiveCountChanged?.Invoke(ActiveCount);
        }

        private GameObject CreateInstance()
        {
            if (prefab == null)
            {
                return null;
            }

            var instance = Instantiate(prefab, transform);
            instance.SetActive(false);

            var pooledInstance = instance.GetComponent<PooledInstance>();
            if (pooledInstance == null)
            {
                pooledInstance = instance.AddComponent<PooledInstance>();
            }

            pooledInstance.SetOwnerPool(this);
            _allInstances.Add(instance);
            return instance;
        }

        private static void NotifyPoolablesSpawned(GameObject instance)
        {
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            foreach (var poolable in poolables)
            {
                poolable.OnSpawned();
            }
        }

        private static void NotifyPoolablesDespawned(GameObject instance)
        {
            var poolables = instance.GetComponentsInChildren<IPoolable>(true);
            foreach (var poolable in poolables)
            {
                poolable.OnDespawned();
            }
        }

        private static void ResetRigidbodyState(GameObject instance)
        {
            var rigidbodies = instance.GetComponentsInChildren<Rigidbody>(true);
            for (var i = 0; i < rigidbodies.Length; i++)
            {
                var rb = rigidbodies[i];
                if (rb == null)
                {
                    continue;
                }

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
            }
        }
    }
}
