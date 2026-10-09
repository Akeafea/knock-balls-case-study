using System;
using System.Collections.Generic;
using KnockBallsCase.Core.Pooling;
using UnityEngine;

namespace KnockBallsCase.Gameplay.LevelSystem.Runtime
{
    public sealed class LevelPoolRegistry : MonoBehaviour
    {
        [SerializeField] private List<PoolEntry> entries = new List<PoolEntry>();

        private readonly Dictionary<string, GameObjectPool> _poolByKey = new Dictionary<string, GameObjectPool>();

        private void Awake()
        {
            RebuildCache();
        }

        public bool TryGetPool(string key, out GameObjectPool pool)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                pool = null;
                return false;
            }

            if (_poolByKey.Count == 0)
            {
                RebuildCache();
            }

            return _poolByKey.TryGetValue(key, out pool) && pool != null;
        }

        private void RebuildCache()
        {
            _poolByKey.Clear();

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key) || entry.Pool == null)
                {
                    continue;
                }

                _poolByKey[entry.Key] = entry.Pool;
            }
        }
    }

    [Serializable]
    public sealed class PoolEntry
    {
        [SerializeField] private string key;
        [SerializeField] private GameObjectPool pool;

        public string Key => key;
        public GameObjectPool Pool => pool;
    }
}
