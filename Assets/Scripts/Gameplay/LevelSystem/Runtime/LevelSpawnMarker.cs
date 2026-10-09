using UnityEngine;

namespace KnockBallsCase.Gameplay.LevelSystem.Runtime
{
    public sealed class LevelSpawnMarker : MonoBehaviour
    {
        public enum SpawnCategory
        {
            Block = 0,
            Platform = 1,
        }

        [SerializeField] private string poolKey;
        [SerializeField] private SpawnCategory category = SpawnCategory.Block;

        public string PoolKey => poolKey;
        public SpawnCategory Category => category;
    }
}
