using System.Collections;
using System.Collections.Generic;
using System;
using KnockBallsCase.Core.Pooling;
using KnockBallsCase.Gameplay.Ball;
using KnockBallsCase.Gameplay.Glass;
using KnockBallsCase.Gameplay.LevelSystem.Data;
using KnockBallsCase.Gameplay.PowerUp;
using KnockBallsCase.Gameplay.State;
using KnockBallsCase.Gameplay.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace KnockBallsCase.Gameplay.LevelSystem.Runtime
{
    public sealed class LevelProgressionManager : MonoBehaviour
    {
        private const string SavedMainLevelKey = "KnockBallsCase.LastMainLevelIndex";
        private const string BestMainLevelScoreKey = "KnockBallsCase.BestMainLevelScore";

        public event Action ProgressionChanged;

        [Header("References")]
        [SerializeField] private LevelPackSO levelPack;
        [SerializeField] private LevelPoolRegistry poolRegistry;
        [SerializeField] private BallThrowController ballThrowController;
        [SerializeField] private PowerUpController powerUpController;
        [SerializeField] private LoseStateController loseStateController;
        [SerializeField] private LevelThemeController levelThemeController;
        [SerializeField] private TMP_Text mainLevelText;
        [SerializeField] private TMP_Text nextLevelText;
        [SerializeField] private TMP_Text miniLevelText;

        [Header("Main Level Finish UI")]
        [SerializeField] private GameObject mainLevelFinishPanel;
        [SerializeField] private TMP_Text mainLevelFinishScoreText;
        [SerializeField] private TMP_Text mainLevelFinishLevelText;

        [Header("All Levels Finish UI")]
        [SerializeField] private GameObject allLevelsFinishPanel;

        [Header("Start Level")]
        [SerializeField] private int startMainLevelIndex;
        [SerializeField] private int startMiniLevelIndex;
        [SerializeField] private bool startFromLastSavedMainLevelOnRestart = true;

        [Header("Events")]
        [SerializeField] private UnityEvent onMiniLevelLoaded;
        [SerializeField] private UnityEvent onMainLevelChanged;
        [SerializeField] private UnityEvent onAllMainLevelsCompleted;

        [Header("Flow")]
        [SerializeField] private float miniLevelCompleteDelay = 0.35f;

        private readonly List<GameObject> _activeDestructibleObjects = new List<GameObject>();
        private readonly List<GameObject> _activePlatformObjects = new List<GameObject>();

        private int _mainLevelIndex;
        private int _miniLevelIndex;
        private bool _initialized;
        private bool _allCompleted;
        private bool _isMiniLevelTransitionScheduled;
        private bool _isWaitingForMainLevelTap;
        private int _pendingMainLevelIndex = -1;
        private int _mainLevelCollectedScore;
        private Coroutine _miniLevelAdvanceRoutine;

        public int CurrentMainLevelIndex => _mainLevelIndex;
        public int CurrentMainLevelNumber => _mainLevelIndex + 1;
        public int CurrentMainLevelScore => Mathf.Max(0, _mainLevelCollectedScore);
        public int CurrentMiniLevelIndex => _miniLevelIndex;
        public bool IsInitialized => _initialized;

        public static int GetBestMainLevelScore()
        {
            return Mathf.Max(0, PlayerPrefs.GetInt(BestMainLevelScoreKey, 0));
        }

        private void OnEnable()
        {
            EnsurePowerUpReference();
            SubscribePowerUpScore();
        }

        private void OnDisable()
        {
            UnsubscribePowerUpScore();
        }

        private void Start()
        {
            if (levelPack == null || levelPack.Levels.Count == 0)
            {
                Debug.LogWarning("LevelProgressionManager has no LevelPack assigned or LevelPack is empty.", this);
                return;
            }

            var initialMainIndex = Mathf.Clamp(startMainLevelIndex, 0, levelPack.Levels.Count - 1);
            if (startFromLastSavedMainLevelOnRestart)
            {
                var savedMainIndex = PlayerPrefs.GetInt(SavedMainLevelKey, initialMainIndex);
                initialMainIndex = Mathf.Clamp(savedMainIndex, 0, levelPack.Levels.Count - 1);
            }

            _mainLevelIndex = initialMainIndex;
            _mainLevelCollectedScore = 0;
            HideMainLevelFinishPanel();
            HideAllLevelsFinishPanel();

            EnsurePowerUpReference();
            SubscribePowerUpScore();

            var selectedMain = levelPack.Levels[_mainLevelIndex];
            var miniCount = selectedMain != null && selectedMain.MiniLevels != null
                ? selectedMain.MiniLevels.Count
                : 0;
            _miniLevelIndex = miniCount > 0
                ? Mathf.Clamp(startMiniLevelIndex, 0, miniCount - 1)
                : 0;

            if (levelThemeController != null)
            {
                levelThemeController.SelectRandomTheme();
            }

            _initialized = true;
            LoadCurrentMiniLevel();
        }

        private void Update()
        {
            if (!_initialized || _allCompleted)
            {
                return;
            }

            if (_isWaitingForMainLevelTap)
            {
                if (WasContinueInputPressed())
                {
                    ContinueToPendingMainLevel();
                }

                return;
            }

            if (_activeDestructibleObjects.Count == 0)
            {
                return;
            }

            var completedNow = AreAllTrackedObjectsInactive(_activeDestructibleObjects) && !HasActiveFracturedGlass();
            if (!completedNow)
            {
                if (_miniLevelAdvanceRoutine != null)
                {
                    StopCoroutine(_miniLevelAdvanceRoutine);
                    _miniLevelAdvanceRoutine = null;
                    _isMiniLevelTransitionScheduled = false;
                }

                return;
            }

            if (_isMiniLevelTransitionScheduled)
            {
                return;
            }

            _miniLevelAdvanceRoutine = StartCoroutine(AdvanceToNextMiniLevelWithDelay());
            _isMiniLevelTransitionScheduled = true;
        }

        public void HandleMiniLevelFailed()
        {
            if (!_initialized || _allCompleted)
            {
                return;
            }

            _miniLevelIndex = 0;
            LoadCurrentMiniLevel();
        }

        private IEnumerator AdvanceToNextMiniLevelWithDelay()
        {
            var delay = Mathf.Max(0f, miniLevelCompleteDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            _miniLevelAdvanceRoutine = null;
            _isMiniLevelTransitionScheduled = false;

            if (_allCompleted || !_initialized)
            {
                yield break;
            }

            if (!AreAllTrackedObjectsInactive(_activeDestructibleObjects) || HasActiveFracturedGlass())
            {
                yield break;
            }

            if (loseStateController != null)
            {
                loseStateController.CancelLoseState();
            }

            AdvanceToNextMiniLevel();
        }

        private void AdvanceToNextMiniLevel()
        {
            var levels = levelPack.Levels;
            if (_mainLevelIndex < 0 || _mainLevelIndex >= levels.Count)
            {
                return;
            }

            var currentMain = levels[_mainLevelIndex];
            var nextMiniIndex = _miniLevelIndex + 1;

            if (nextMiniIndex < currentMain.MiniLevels.Count)
            {
                _miniLevelIndex = nextMiniIndex;
                LoadCurrentMiniLevel();
                return;
            }

            var nextMainIndex = _mainLevelIndex + 1;
            if (nextMainIndex < levels.Count)
            {
                ShowMainLevelFinishPanel(nextMainIndex);
                return;
            }

            _allCompleted = true;
            ShowAllLevelsFinishPanel();
            ProgressionChanged?.Invoke();
            onAllMainLevelsCompleted?.Invoke();
        }

        private void LoadCurrentMiniLevel()
        {
            if (_miniLevelAdvanceRoutine != null)
            {
                StopCoroutine(_miniLevelAdvanceRoutine);
                _miniLevelAdvanceRoutine = null;
            }

            _isMiniLevelTransitionScheduled = false;

            if (loseStateController != null)
            {
                loseStateController.CancelLoseState();
            }

            ClearCurrentMiniLevelObjects();

            var miniLevel = GetCurrentMiniLevel();
            if (miniLevel == null)
            {
                _allCompleted = true;
                return;
            }

            SpawnInstructions(miniLevel.PlatformSpawns, _activePlatformObjects);
            SpawnInstructions(miniLevel.BlockSpawns, _activeDestructibleObjects);

            if (levelThemeController != null)
            {
                levelThemeController.ApplyCurrentThemeToLevel(_activePlatformObjects, _activeDestructibleObjects);
            }

            if (ballThrowController != null)
            {
                ballThrowController.ResetForNewMiniLevel(miniLevel.AmmoCount);
            }

            UpdateLevelTexts();
            SaveCurrentMainLevel();
            ProgressionChanged?.Invoke();
            onMiniLevelLoaded?.Invoke();

            if (_activeDestructibleObjects.Count == 0)
            {
                AdvanceToNextMiniLevel();
            }
        }

        private void ShowMainLevelFinishPanel(int nextMainIndex)
        {
            _pendingMainLevelIndex = nextMainIndex;
            _isWaitingForMainLevelTap = true;

            var collected = CurrentMainLevelScore;
            if (mainLevelFinishScoreText != null)
            {
                mainLevelFinishScoreText.text = collected.ToString();
            }

            if (mainLevelFinishLevelText != null)
            {
                mainLevelFinishLevelText.text = (_mainLevelIndex + 1).ToString();
            }

            if (mainLevelFinishPanel != null)
            {
                mainLevelFinishPanel.SetActive(true);
            }
        }

        private void ContinueToPendingMainLevel()
        {
            if (!_isWaitingForMainLevelTap)
            {
                return;
            }

            var nextMainIndex = _pendingMainLevelIndex;
            _pendingMainLevelIndex = -1;
            _isWaitingForMainLevelTap = false;
            HideMainLevelFinishPanel();

            if (nextMainIndex < 0 || nextMainIndex >= levelPack.Levels.Count)
            {
                return;
            }

            _mainLevelIndex = nextMainIndex;
            _miniLevelIndex = 0;
            _mainLevelCollectedScore = 0;

            if (levelThemeController != null)
            {
                levelThemeController.SelectRandomTheme();
            }

            onMainLevelChanged?.Invoke();
            ProgressionChanged?.Invoke();
            LoadCurrentMiniLevel();
        }

        private void HideMainLevelFinishPanel()
        {
            if (mainLevelFinishPanel != null)
            {
                mainLevelFinishPanel.SetActive(false);
            }
        }

        private void ShowAllLevelsFinishPanel()
        {
            if (allLevelsFinishPanel != null)
            {
                allLevelsFinishPanel.SetActive(true);
            }
        }

        private void HideAllLevelsFinishPanel()
        {
            if (allLevelsFinishPanel != null)
            {
                allLevelsFinishPanel.SetActive(false);
            }
        }

        private static bool WasContinueInputPressed()
        {
            if (Input.GetMouseButtonDown(0))
            {
                return true;
            }

            if (Input.touchCount <= 0)
            {
                return false;
            }

            for (var i = 0; i < Input.touchCount; i++)
            {
                if (Input.GetTouch(i).phase == TouchPhase.Began)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsurePowerUpReference()
        {
            if (powerUpController != null)
            {
                return;
            }

            powerUpController = FindAnyObjectByType<PowerUpController>();
        }

        private void SubscribePowerUpScore()
        {
            if (powerUpController != null)
            {
                powerUpController.ScoreAdded -= OnScoreAdded;
                powerUpController.ScoreAdded += OnScoreAdded;
            }
        }

        private void UnsubscribePowerUpScore()
        {
            if (powerUpController != null)
            {
                powerUpController.ScoreAdded -= OnScoreAdded;
            }
        }

        private void OnScoreAdded(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _mainLevelCollectedScore += amount;
            TrySetBestMainLevelScore(_mainLevelCollectedScore);
        }

        private static void TrySetBestMainLevelScore(int score)
        {
            var normalized = Mathf.Max(0, score);
            var currentBest = GetBestMainLevelScore();
            if (normalized <= currentBest)
            {
                return;
            }

            PlayerPrefs.SetInt(BestMainLevelScoreKey, normalized);
            PlayerPrefs.Save();
        }

        private void SpawnInstructions(IReadOnlyList<SpawnInstruction> instructions, List<GameObject> targetList)
        {
            if (instructions == null)
            {
                return;
            }

            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                if (instruction == null || string.IsNullOrWhiteSpace(instruction.PoolKey))
                {
                    continue;
                }

                if (poolRegistry == null || !poolRegistry.TryGetPool(instruction.PoolKey, out var pool) || pool == null)
                {
                    Debug.LogWarning($"Pool key '{instruction.PoolKey}' could not be resolved for mini level spawn.", this);
                    continue;
                }

                var instance = pool.Rent();
                if (instance == null)
                {
                    continue;
                }

                var tr = instance.transform;
                tr.SetPositionAndRotation(instruction.Position, Quaternion.Euler(instruction.RotationEuler));
                tr.localScale = instruction.Scale;

                targetList.Add(instance);
            }
        }

        private void ClearCurrentMiniLevelObjects()
        {
            ReturnTrackedObjects(_activeDestructibleObjects);
            ReturnTrackedObjects(_activePlatformObjects);

            _activeDestructibleObjects.Clear();
            _activePlatformObjects.Clear();
        }

        private static void ReturnTrackedObjects(List<GameObject> tracked)
        {
            for (var i = 0; i < tracked.Count; i++)
            {
                var obj = tracked[i];
                if (obj == null)
                {
                    continue;
                }

                var pooled = obj.GetComponent<PooledInstance>();
                if (pooled != null)
                {
                    pooled.ReturnToPool();
                }
                else
                {
                    obj.SetActive(false);
                }
            }
        }

        private static bool AreAllTrackedObjectsInactive(List<GameObject> tracked)
        {
            for (var i = 0; i < tracked.Count; i++)
            {
                var obj = tracked[i];
                if (obj != null && obj.activeInHierarchy)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasActiveFracturedGlass()
        {
            return FracturedGlassBehaviour.ActiveFractureCount > 0;
        }

        private MiniLevelData GetCurrentMiniLevel()
        {
            if (levelPack == null)
            {
                return null;
            }

            var levels = levelPack.Levels;
            if (_mainLevelIndex < 0 || _mainLevelIndex >= levels.Count)
            {
                return null;
            }

            var main = levels[_mainLevelIndex];
            if (main == null || main.MiniLevels == null)
            {
                return null;
            }

            if (_miniLevelIndex < 0 || _miniLevelIndex >= main.MiniLevels.Count)
            {
                return null;
            }

            return main.MiniLevels[_miniLevelIndex];
        }

        public int GetCurrentMainMiniLevelCount()
        {
            if (levelPack == null || levelPack.Levels == null)
            {
                return 0;
            }

            if (_mainLevelIndex < 0 || _mainLevelIndex >= levelPack.Levels.Count)
            {
                return 0;
            }

            var main = levelPack.Levels[_mainLevelIndex];
            return main != null && main.MiniLevels != null ? main.MiniLevels.Count : 0;
        }

        private void UpdateLevelTexts()
        {
            if (levelPack == null || levelPack.Levels.Count == 0)
            {
                return;
            }

            var currentMain = levelPack.Levels[_mainLevelIndex];
            if (mainLevelText != null)
            {
                mainLevelText.text = (_mainLevelIndex + 1).ToString();
            }

            if (miniLevelText != null)
            {
                var currentMini = GetCurrentMiniLevel();
                miniLevelText.text = currentMini != null && !string.IsNullOrWhiteSpace(currentMini.Id)
                    ? currentMini.Id
                    : "Mini " + (_miniLevelIndex + 1);
            }

            if (nextLevelText != null)
            {
                var nextMainIndex = _mainLevelIndex + 1;
                if (nextMainIndex >= 0 && nextMainIndex < levelPack.Levels.Count)
                {
                    nextLevelText.text = (nextMainIndex + 1).ToString();
                }
                else
                {
                    nextLevelText.text = "-";
                }
            }
        }

        private void SaveCurrentMainLevel()
        {
            if (!startFromLastSavedMainLevelOnRestart)
            {
                return;
            }

            PlayerPrefs.SetInt(SavedMainLevelKey, _mainLevelIndex);
            PlayerPrefs.Save();
        }

        public void OnResetProgressAndGoMenuButtonPressed()
        {
            PlayerPrefs.DeleteKey(SavedMainLevelKey);
            PlayerPrefs.Save();

            Time.timeScale = 1f;
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
