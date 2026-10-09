using System;
using System.Collections.Generic;
using UnityEngine;

namespace KnockBallsCase.Gameplay.LevelSystem.Data
{
    [CreateAssetMenu(fileName = "LevelPack", menuName = "KnockBalls/Level Pack")]
    public sealed class LevelPackSO : ScriptableObject
    {
        [SerializeField] private List<MainLevelData> levels = new List<MainLevelData>();

        public IReadOnlyList<MainLevelData> Levels => levels;
    }

    [Serializable]
    public sealed class MainLevelData
    {
        [SerializeField] private string id = "MainLevel";
        [SerializeField] private List<MiniLevelData> miniLevels = new List<MiniLevelData>();

        public string Id => id;
        public IReadOnlyList<MiniLevelData> MiniLevels => miniLevels;
    }

    [Serializable]
    public sealed class MiniLevelData
    {
        [SerializeField] private string id = "MiniLevel";
        [SerializeField] private int ammoCount = 5;
        [SerializeField] private List<SpawnInstruction> blockSpawns = new List<SpawnInstruction>();
        [SerializeField] private List<SpawnInstruction> platformSpawns = new List<SpawnInstruction>();

        public string Id => id;
        public int AmmoCount => ammoCount;
        public IReadOnlyList<SpawnInstruction> BlockSpawns => blockSpawns;
        public IReadOnlyList<SpawnInstruction> PlatformSpawns => platformSpawns;
    }

    [Serializable]
    public sealed class SpawnInstruction
    {
        [SerializeField] private string poolKey;
        [SerializeField] private Vector3 position;
        [SerializeField] private Vector3 rotationEuler;
        [SerializeField] private Vector3 scale = Vector3.one;

        public string PoolKey => poolKey;
        public Vector3 Position => position;
        public Vector3 RotationEuler => rotationEuler;
        public Vector3 Scale => scale;
    }
}
