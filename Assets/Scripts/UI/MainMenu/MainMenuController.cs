using KnockBallsCase.Gameplay.LevelSystem.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KnockBallsCase.UI.MainMenu
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject playgroundUI;
        [SerializeField] private Button tapToPlayButton;
        [SerializeField] private TMP_Text bestScoreText;
        [SerializeField] private string bestScorePrefix = "Best: ";

        [Header("Input")]
        [SerializeField] private float gameplayInputBlockAfterCloseSeconds = 0.12f;

        private static float _gameplayInputBlockedUntilTime;

        public static bool IsMenuOpen { get; private set; }

        public static bool IsGameplayInputBlocked
        {
            get
            {
                return Time.unscaledTime < _gameplayInputBlockedUntilTime;
            }
        }

        private void Start()
        {
            ShowMenu();
        }

        private void OnEnable()
        {
            if (tapToPlayButton != null)
            {
                tapToPlayButton.onClick.AddListener(StartGameFromButton);
            }
        }

        private void OnDisable()
        {
            if (tapToPlayButton != null)
            {
                tapToPlayButton.onClick.RemoveListener(StartGameFromButton);
            }
        }

        private void ShowMenu()
        {
            IsMenuOpen = true;
            // Menu açıkken gameplay akışı tamamen durur.
            Time.timeScale = 0f;

            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }

            if (playgroundUI != null)
            {
                playgroundUI.SetActive(false);
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = bestScorePrefix + LevelProgressionManager.GetBestMainLevelScore();
            }
        }

        public void StartGameFromButton()
        {
            if (!IsMenuOpen)
            {
                return;
            }

            HideMenuAndResume();
        }

        public void GoHomeAndReset()
        {
            Time.timeScale = 1f;
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }

        private void HideMenuAndResume()
        {
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(false);
            }

            if (playgroundUI != null)
            {
                playgroundUI.SetActive(true);
            }

            // İlk gameplay click'inde yanlışlıkla ateş edilmesini engeller.
            _gameplayInputBlockedUntilTime = Time.unscaledTime + Mathf.Max(0f, gameplayInputBlockAfterCloseSeconds);

            IsMenuOpen = false;
            Time.timeScale = 1f;
        }
    }
}
