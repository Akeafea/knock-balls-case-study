using System.Collections.Generic;
using KnockBallsCase.Gameplay.LevelSystem.Data;
using KnockBallsCase.Gameplay.LevelSystem.Runtime;
using UnityEditor;
using UnityEngine;

namespace KnockBallsCase.Editor.LevelSystem
{
    public sealed class LevelLayoutExporterWindow : EditorWindow
    {
        private LevelPackSO _levelPack;
        private int _mainLevelIndex;
        private int _miniLevelIndex;
        private bool _includeInactive = true;

        [MenuItem("Tools/KnockBalls/Level Layout Exporter")]
        private static void Open()
        {
            GetWindow<LevelLayoutExporterWindow>("Level Exporter");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scene -> Mini Level Export", EditorStyles.boldLabel);

            _levelPack = (LevelPackSO)EditorGUILayout.ObjectField("Level Pack", _levelPack, typeof(LevelPackSO), false);
            _mainLevelIndex = Mathf.Max(0, EditorGUILayout.IntField("Main Level Index", _mainLevelIndex));
            _miniLevelIndex = Mathf.Max(0, EditorGUILayout.IntField("Mini Level Index", _miniLevelIndex));
            _includeInactive = EditorGUILayout.Toggle("Include Inactive", _includeInactive);

            EditorGUILayout.Space(8f);

            using (new EditorGUI.DisabledScope(_levelPack == null))
            {
                if (GUILayout.Button("Export Markers To Mini Level"))
                {
                    Export();
                }
            }

            EditorGUILayout.HelpBox(
                "1) Scene objelerine LevelSpawnMarker ekle.\n" +
                "2) poolKey doldur, category (Block/Platform) sec.\n" +
                "3) LevelPack + index secip export et.",
                MessageType.Info);
        }

        private void Export()
        {
            if (_levelPack == null)
            {
                Debug.LogWarning("LevelPack is null.");
                return;
            }

            var serialized = new SerializedObject(_levelPack);
            var levels = serialized.FindProperty("levels");
            if (levels == null || _mainLevelIndex >= levels.arraySize)
            {
                Debug.LogWarning("Main level index is out of range.");
                return;
            }

            var mainLevel = levels.GetArrayElementAtIndex(_mainLevelIndex);
            var miniLevels = mainLevel.FindPropertyRelative("miniLevels");
            if (miniLevels == null || _miniLevelIndex >= miniLevels.arraySize)
            {
                Debug.LogWarning("Mini level index is out of range.");
                return;
            }

            var miniLevel = miniLevels.GetArrayElementAtIndex(_miniLevelIndex);
            var blockSpawns = miniLevel.FindPropertyRelative("blockSpawns");
            var platformSpawns = miniLevel.FindPropertyRelative("platformSpawns");
            if (blockSpawns == null || platformSpawns == null)
            {
                Debug.LogWarning("Spawn arrays could not be found on MiniLevelData.");
                return;
            }

            blockSpawns.ClearArray();
            platformSpawns.ClearArray();

            var markers = FindMarkers();
            var blockCount = 0;
            var platformCount = 0;

            for (var i = 0; i < markers.Count; i++)
            {
                var marker = markers[i];
                if (marker == null || string.IsNullOrWhiteSpace(marker.PoolKey))
                {
                    continue;
                }

                var targetArray = marker.Category == LevelSpawnMarker.SpawnCategory.Block
                    ? blockSpawns
                    : platformSpawns;

                var index = targetArray.arraySize;
                targetArray.InsertArrayElementAtIndex(index);
                var element = targetArray.GetArrayElementAtIndex(index);

                element.FindPropertyRelative("poolKey").stringValue = marker.PoolKey;
                element.FindPropertyRelative("position").vector3Value = marker.transform.position;
                element.FindPropertyRelative("rotationEuler").vector3Value = marker.transform.rotation.eulerAngles;
                element.FindPropertyRelative("scale").vector3Value = marker.transform.lossyScale;

                if (marker.Category == LevelSpawnMarker.SpawnCategory.Block)
                {
                    blockCount++;
                }
                else
                {
                    platformCount++;
                }
            }

            serialized.ApplyModifiedProperties();
            EditorUtility.SetDirty(_levelPack);
            AssetDatabase.SaveAssets();

            Debug.Log($"Export complete. Main:{_mainLevelIndex} Mini:{_miniLevelIndex} | Blocks:{blockCount} Platforms:{platformCount}");
        }

        private List<LevelSpawnMarker> FindMarkers()
        {
            var all = Object.FindObjectsByType<LevelSpawnMarker>(
                _includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude);

            var list = new List<LevelSpawnMarker>(all.Length);
            for (var i = 0; i < all.Length; i++)
            {
                if (all[i] != null)
                {
                    list.Add(all[i]);
                }
            }

            return list;
        }
    }
}
