using System;
using System.Collections;
using KnockBallsCase.Gameplay.Ball;
using KnockBallsCase.Gameplay.Block;
using KnockBallsCase.Gameplay.Explosive;
using KnockBallsCase.Gameplay.Glass;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KnockBallsCase.Gameplay.PowerUp
{
    public sealed class PowerUpController : MonoBehaviour, ISpecialFireModeHandler
    {
        public event Action<int> ScoreAdded;
        public int TotalScore => _totalScore;

        [Header("References")]
        [SerializeField] private BallThrowController ballThrowController;
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Animator scoreTextAnimator;
        [SerializeField] private Image powerUpProgressFillImage;
        [SerializeField] private Button powerUpButton;

        [Header("Score")]
        [SerializeField] private int scorePerDestroyedBlock = 1;
        [SerializeField] private int scorePerGlassBlock = 3;
        [SerializeField] private int scoreNeededForPowerUp = 10;

        [Header("Score Text FX")]
        [SerializeField] private float scoreTextUpdateDelay = 0.3f;
        [SerializeField] private string scoreTextPopTriggerName = "Pop";

        [Header("PowerUp")]
        [SerializeField] private int powerUpShotCount = 10;
        [SerializeField] private float powerUpShotDelay = 0.08f;

        private int _totalScore;
        private int _powerUpChargeScore;
        private bool _powerUpReady;
        private bool _powerUpArmed;
        private bool _isBurstRunning;
        private Coroutine _burstRoutine;
        private Coroutine _scoreTextDelayRoutine;
        private int _displayedScore;

        private void OnEnable()
        {
            _totalScore = 0;
            _powerUpChargeScore = 0;
            _displayedScore = 0;
            _powerUpReady = false;
            _powerUpArmed = false;
            _isBurstRunning = false;

            if (_burstRoutine != null)
            {
                StopCoroutine(_burstRoutine);
                _burstRoutine = null;
            }

            if (_scoreTextDelayRoutine != null)
            {
                StopCoroutine(_scoreTextDelayRoutine);
                _scoreTextDelayRoutine = null;
            }

            BlockGroundRecycle.BlockDestroyed += OnBlockDestroyed;
            GlassBlockFractureOnHit.GlassBroken += OnGlassBroken;
            ExplosiveBarrelBlock.Exploded += OnExplosiveBarrelExploded;

            if (powerUpButton != null)
            {
                powerUpButton.onClick.AddListener(OnPowerUpButtonPressed);
            }

            RefreshUI();
        }

        private void OnDisable()
        {
            BlockGroundRecycle.BlockDestroyed -= OnBlockDestroyed;
            GlassBlockFractureOnHit.GlassBroken -= OnGlassBroken;
            ExplosiveBarrelBlock.Exploded -= OnExplosiveBarrelExploded;

            if (powerUpButton != null)
            {
                powerUpButton.onClick.RemoveListener(OnPowerUpButtonPressed);
            }

            if (_burstRoutine != null)
            {
                StopCoroutine(_burstRoutine);
                _burstRoutine = null;
            }

            if (_scoreTextDelayRoutine != null)
            {
                StopCoroutine(_scoreTextDelayRoutine);
                _scoreTextDelayRoutine = null;
            }
        }

        public bool TryHandlePrimaryFire()
        {
            if (_isBurstRunning)
            {
                return true;
            }

            if (!_powerUpArmed)
            {
                return false;
            }

            if (ballThrowController == null)
            {
                // Power-up is armed, so never fall back to a normal ammo shot.
                return true;
            }

            if (_burstRoutine != null)
            {
                StopCoroutine(_burstRoutine);
            }

            _burstRoutine = StartCoroutine(FirePowerUpBurst());
            return true;
        }

        public void OnPowerUpButtonPressed()
        {
            if (!_powerUpReady || _powerUpArmed || _isBurstRunning)
            {
                return;
            }

            _powerUpArmed = true;
            _powerUpReady = false;
            _powerUpChargeScore = 0;
            RefreshUI();
        }

        private void OnBlockDestroyed()
        {
            AddScore(scorePerDestroyedBlock);
        }

        private void OnGlassBroken()
        {
            AddScore(scorePerGlassBlock);
        }

        private void OnExplosiveBarrelExploded(int scoreAmount)
        {
            AddScore(scoreAmount);
        }

        private void AddScore(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _totalScore += amount;
            ScoreAdded?.Invoke(amount);
            ScheduleDelayedScoreTextRefresh();

            // Keep total score independent, but do not build power-up charge while consuming power-up.
            if (_powerUpArmed || _isBurstRunning)
            {
                RefreshUI();
                return;
            }

            _powerUpChargeScore += amount;

            var needed = Mathf.Max(1, scoreNeededForPowerUp);
            if (_powerUpChargeScore >= needed)
            {
                _powerUpChargeScore = needed;
                _powerUpReady = true;
            }

            RefreshUI();
        }

        private IEnumerator FirePowerUpBurst()
        {
            _isBurstRunning = true;
            RefreshUI();

            var burstCount = Mathf.Max(1, powerUpShotCount);
            for (var i = 0; i < burstCount; i++)
            {
                if (ballThrowController != null)
                {
                    var currentDirection = ballThrowController.GetShootDirectionForCurrentInput();
                    ballThrowController.TryFireFromDirection(currentDirection, false);
                }

                if (i < burstCount - 1)
                {
                    var wait = Mathf.Max(0f, powerUpShotDelay);
                    if (wait > 0f)
                    {
                        yield return new WaitForSeconds(wait);
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }

            _powerUpArmed = false;
            _isBurstRunning = false;
            _burstRoutine = null;
            RefreshUI();
        }

        private void RefreshUI()
        {
            UpdateScoreTextVisual();

            if (powerUpProgressFillImage != null)
            {
                var needed = Mathf.Max(1, scoreNeededForPowerUp);
                powerUpProgressFillImage.fillAmount = Mathf.Clamp01((float)_powerUpChargeScore / needed);
            }

            if (powerUpButton != null)
            {
                powerUpButton.interactable = _powerUpReady && !_powerUpArmed && !_isBurstRunning;
            }
        }

        private void ScheduleDelayedScoreTextRefresh()
        {
            if (_scoreTextDelayRoutine != null)
            {
                StopCoroutine(_scoreTextDelayRoutine);
            }

            _scoreTextDelayRoutine = StartCoroutine(DelayedScoreTextRefreshRoutine());
        }

        private IEnumerator DelayedScoreTextRefreshRoutine()
        {
            var delay = Mathf.Max(0f, scoreTextUpdateDelay);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            _displayedScore = _totalScore;
            UpdateScoreTextVisual();
            TriggerScoreTextPop();
            _scoreTextDelayRoutine = null;
        }

        private void SetDisplayedScoreImmediate(int value, bool triggerPop)
        {
            _displayedScore = Mathf.Max(0, value);
            UpdateScoreTextVisual();

            if (triggerPop)
            {
                TriggerScoreTextPop();
            }
        }

        private void UpdateScoreTextVisual()
        {
            if (scoreText != null)
            {
                scoreText.text = _displayedScore.ToString();
            }
        }

        private void TriggerScoreTextPop()
        {
            if (scoreTextAnimator == null || string.IsNullOrWhiteSpace(scoreTextPopTriggerName))
            {
                return;
            }

            scoreTextAnimator.SetTrigger(scoreTextPopTriggerName);
        }
    }
}
