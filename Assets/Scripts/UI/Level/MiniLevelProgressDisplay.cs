using System.Collections.Generic;
using KnockBallsCase.Gameplay.LevelSystem.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace KnockBallsCase.UI.Level
{
    public sealed class MiniLevelProgressDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelProgressionManager levelProgressionManager;
        [SerializeField] private List<Image> miniLevelBars = new List<Image>();

        [Header("Colors")]
        [SerializeField] private Color completedColor = new Color(1f, 0.5f, 0f, 1f);
        [SerializeField] private Color currentColor = new Color(1f, 0.85f, 0f, 1f);
        [SerializeField] private Color defaultColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Header("Behavior")]
        [SerializeField] private bool hideUnusedBars = true;

        private bool _isSubscribed;
        private bool _hasThemeColors;
        private Color _themeCompleted, _themeCurrent, _themeDefault;

        public void SetColors(Color completed, Color current, Color pending)
        {
            _hasThemeColors = true;
            _themeCompleted = completed;
            _themeCurrent = current;
            _themeDefault = pending;
            if (Application.isPlaying) Refresh();
            else
            {
                for (var i = 0; i < miniLevelBars.Count; i++)
                    if (miniLevelBars[i] != null) miniLevelBars[i].color = i == 0 ? current : pending;
            }
        }

        public void RestoreColors()
        {
            _hasThemeColors = false;
            if (Application.isPlaying) Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Refresh()
        {
            if (levelProgressionManager == null)
            {
                return;
            }

            var totalMini = levelProgressionManager.GetCurrentMainMiniLevelCount();
            var currentMini = levelProgressionManager.CurrentMiniLevelIndex;

            for (var i = 0; i < miniLevelBars.Count; i++)
            {
                var bar = miniLevelBars[i];
                if (bar == null)
                {
                    continue;
                }

                var inRange = i < totalMini;
                if (!inRange && hideUnusedBars)
                {
                    bar.gameObject.SetActive(false);
                    continue;
                }

                bar.gameObject.SetActive(true);

                if (!inRange)
                {
                    bar.color = _hasThemeColors ? _themeDefault : defaultColor;
                    continue;
                }

                if (i < currentMini)
                {
                    bar.color = _hasThemeColors ? _themeCompleted : completedColor;
                }
                else if (i == currentMini)
                {
                    bar.color = _hasThemeColors ? _themeCurrent : currentColor;
                }
                else
                {
                    bar.color = _hasThemeColors ? _themeDefault : defaultColor;
                }
            }
        }

        private void Subscribe()
        {
            if (_isSubscribed || levelProgressionManager == null)
            {
                return;
            }

            levelProgressionManager.ProgressionChanged += Refresh;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed || levelProgressionManager == null)
            {
                return;
            }

            levelProgressionManager.ProgressionChanged -= Refresh;
            _isSubscribed = false;
        }
    }
}
