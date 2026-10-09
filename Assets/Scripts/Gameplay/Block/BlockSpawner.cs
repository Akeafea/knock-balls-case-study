using System.Collections.Generic;
using KnockBallsCase.Core.Pooling;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Block
{
    public sealed class BlockSpawner : MonoBehaviour
    {
        [SerializeField] private GameObjectPool blockPool;
        [SerializeField] private int blockCount = 20;
        [SerializeField] private Vector3 areaSize = new Vector3(8f, 0f, 8f);
        [SerializeField] private float spawnHeight = 3f;

        private readonly List<GameObject> _spawnedBlocks = new List<GameObject>();

        private void Start()
        {
            SpawnAll();
        }

        public void SpawnAll()
        {
            if (blockPool == null)
            {
                return;
            }

            ClearSpawned();

            for (var i = 0; i < blockCount; i++)
            {
                var block = blockPool.Rent();
                if (block == null)
                {
                    break;
                }

                var pos = GetRandomSpawnPosition();
                block.transform.position = pos;
                block.transform.rotation = Quaternion.identity;

                _spawnedBlocks.Add(block);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            var center = transform.position;
            var randomX = Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f);
            var randomZ = Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f);
            return new Vector3(center.x + randomX, center.y + spawnHeight, center.z + randomZ);
        }

        private void ClearSpawned()
        {
            for (var i = 0; i < _spawnedBlocks.Count; i++)
            {
                var block = _spawnedBlocks[i];
                if (block != null)
                {
                    blockPool.Return(block);
                }
            }

            _spawnedBlocks.Clear();
        }
    }
}
