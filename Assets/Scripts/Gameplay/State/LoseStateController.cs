using System.Collections;
using KnockBallsCase.Gameplay.LevelSystem.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace KnockBallsCase.Gameplay.State
{
    public sealed class LoseStateController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject loseAnimObject;
        [SerializeField] private GameObject loseCanvasObject;
        [SerializeField] private GameObject defaultBackObject;
        [SerializeField] private GameObject restartButtonObject;
        [SerializeField] private TMP_Text loseDurationText;
        [SerializeField] private TMP_Text failCurrentLevelText;
        [SerializeField] private TMP_Text failScoreText;
        [SerializeField] private LevelProgressionManager levelProgressionManager;

        [Header("Timing")]
        [SerializeField] private float loseScreenDuration = 5f;

        private Coroutine _loseRoutine;

        public void EnterLoseState()
        {
            RefreshFailInfo();

            if (_loseRoutine != null)
            {
                StopCoroutine(_loseRoutine);
            }

            _loseRoutine = StartCoroutine(LoseFlowRoutine());
        }

        public void CancelLoseState()
        {
            if (_loseRoutine != null)
            {
                StopCoroutine(_loseRoutine);
                _loseRoutine = null;
            }

            SetActive(loseAnimObject, false);
            SetActive(defaultBackObject, false);
            SetActive(restartButtonObject, false);
            SetActive(loseCanvasObject, false);

            if (loseDurationText != null)
            {
                loseDurationText.gameObject.SetActive(false);
                UpdateLoseDurationText(0f);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            var activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.buildIndex);
        }

        private IEnumerator LoseFlowRoutine()
        {
            SetActive(loseCanvasObject, true);
            if (loseDurationText != null)
            {
                loseDurationText.gameObject.SetActive(true);
            }

            SetActive(defaultBackObject, false);
            SetActive(restartButtonObject, false);

            SetActive(loseAnimObject, true);
            RestartAnimators(loseAnimObject);

            var remaining = Mathf.Max(0f, loseScreenDuration);
            while (remaining > 0f)
            {
                UpdateLoseDurationText(remaining);
                remaining -= Time.deltaTime;
                yield return null;
            }

            UpdateLoseDurationText(0f);
            if (loseDurationText != null)
            {
                loseDurationText.gameObject.SetActive(false);
            }

            SetActive(loseAnimObject, false);
            UpdateLoseDurationText(0f);

            SetActive(defaultBackObject, true);
            RestartAnimators(defaultBackObject);

            SetActive(restartButtonObject, true);
            RestartAnimators(restartButtonObject);

            _loseRoutine = null;
        }

        private void UpdateLoseDurationText(float seconds)
        {
            if (loseDurationText == null)
            {
                return;
            }

            loseDurationText.text = Mathf.Max(0f, seconds).ToString("F1") + "s";
        }

        private void RefreshFailInfo()
        {
            if (levelProgressionManager == null)
            {
                levelProgressionManager = FindAnyObjectByType<LevelProgressionManager>();
            }

            if (levelProgressionManager != null)
            {
                if (failCurrentLevelText != null)
                {
                    failCurrentLevelText.text = "Level " + levelProgressionManager.CurrentMainLevelNumber;
                }

                if (failScoreText != null)
                {
                    failScoreText.text = levelProgressionManager.CurrentMainLevelScore.ToString();
                }

                return;
            }

            if (failCurrentLevelText != null)
            {
                failCurrentLevelText.text = "Level -";
            }

            if (failScoreText != null)
            {
                failScoreText.text = "0";
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static void RestartAnimators(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            var animators = root.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var animator = animators[i];
                if (animator == null)
                {
                    continue;
                }

                animator.Rebind();
                animator.Update(0f);
            }
        }
    }
}
