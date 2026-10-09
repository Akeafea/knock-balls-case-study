using System.Collections;
using System.Collections.Generic;
using KnockBallsCase.Core.Pooling;
using KnockBallsCase.Gameplay.PowerUp;
using TMPro;
using UnityEngine;

namespace KnockBallsCase.UI.Score
{
    public sealed class ScoreGainTextPoolController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PowerUpController powerUpController;
        [SerializeField] private GameObjectPool textPool;
        [SerializeField] private Transform textSpawnParent;

        [Header("Timing")]
        [SerializeField] private float textActiveDuration = 1f;

        [Header("Colors")]
        [SerializeField] private Color plusOneColor = Color.white;
        [SerializeField] private Color plusThreeColor = Color.yellow;
        [SerializeField] private Color plusFiveColor = new Color(1f, 0.5f, 0f, 1f);

        private readonly Dictionary<GameObject, int> _spawnVersionByObject = new Dictionary<GameObject, int>();

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            StopAllCoroutines();
            _spawnVersionByObject.Clear();
        }

        private void Subscribe()
        {
            if (powerUpController != null)
            {
                powerUpController.ScoreAdded += HandleScoreAdded;
            }
        }

        private void Unsubscribe()
        {
            if (powerUpController != null)
            {
                powerUpController.ScoreAdded -= HandleScoreAdded;
            }
        }

        private void HandleScoreAdded(int amount)
        {
            if (amount <= 0 || textPool == null)
            {
                return;
            }

            var textObject = textPool.Rent();
            if (textObject == null)
            {
                return;
            }

            if (textSpawnParent != null)
            {
                textObject.transform.SetParent(textSpawnParent, false);
            }

            AlignToCanvasCenter(textObject.transform, textSpawnParent);

            ResetVisualState(textObject);

            var scoreText = textObject.GetComponentInChildren<TMP_Text>(true);
            if (scoreText != null)
            {
                scoreText.text = $"+{amount}";
                scoreText.color = ResolveColor(amount);
            }

            var version = 1;
            if (_spawnVersionByObject.TryGetValue(textObject, out var existingVersion))
            {
                version = existingVersion + 1;
            }

            _spawnVersionByObject[textObject] = version;
            StartCoroutine(ReturnToPoolAfterDelay(textObject, version));
        }

        private IEnumerator ReturnToPoolAfterDelay(GameObject textObject, int version)
        {
            var delay = Mathf.Max(0f, textActiveDuration);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            if (textObject == null)
            {
                yield break;
            }

            if (!_spawnVersionByObject.TryGetValue(textObject, out var currentVersion) || currentVersion != version)
            {
                yield break;
            }

            var pooled = textObject.GetComponent<PooledInstance>();
            if (pooled != null)
            {
                pooled.ReturnToPool();
            }
            else
            {
                textObject.SetActive(false);
            }
        }

        private Color ResolveColor(int amount)
        {
            if (amount == 1)
            {
                return plusOneColor;
            }

            if (amount == 3)
            {
                return plusThreeColor;
            }

            if (amount == 5)
            {
                return plusFiveColor;
            }

            return plusOneColor;
        }

        private static void AlignToCanvasCenter(Transform target, Transform spawnParent)
        {
            if (target == null)
            {
                return;
            }

            var targetRect = target as RectTransform;
            if (targetRect != null)
            {
                var canvas = target.GetComponentInParent<Canvas>();
                if (canvas == null && spawnParent != null)
                {
                    canvas = spawnParent.GetComponentInParent<Canvas>();
                }

                if (canvas != null)
                {
                    var canvasRect = canvas.transform as RectTransform;
                    if (canvasRect != null)
                    {
                        if (targetRect.parent != canvasRect)
                        {
                            targetRect.SetParent(canvasRect, false);
                        }

                        targetRect.anchorMin = new Vector2(0.5f, 0.5f);
                        targetRect.anchorMax = new Vector2(0.5f, 0.5f);
                        targetRect.pivot = new Vector2(0.5f, 0.5f);
                        targetRect.anchoredPosition = Vector2.zero;
                        targetRect.localRotation = Quaternion.identity;
                        targetRect.localScale = Vector3.one;
                        return;
                    }
                }

                targetRect.anchoredPosition = Vector2.zero;
                targetRect.localRotation = Quaternion.identity;
                targetRect.localScale = Vector3.one;
                return;
            }

            if (spawnParent != null)
            {
                target.position = spawnParent.position;
                target.rotation = spawnParent.rotation;
            }
        }

        private static void ResetVisualState(GameObject textObject)
        {
            if (textObject == null)
            {
                return;
            }

            var canvasGroup = textObject.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            var allTexts = textObject.GetComponentsInChildren<TMP_Text>(true);
            for (var i = 0; i < allTexts.Length; i++)
            {
                if (allTexts[i] == null)
                {
                    continue;
                }

                var c = allTexts[i].color;
                c.a = 1f;
                allTexts[i].color = c;
            }

            var animators = textObject.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var animator = animators[i];
                if (animator == null)
                {
                    continue;
                }

                if (animator.runtimeAnimatorController == null)
                {
                    continue;
                }

                animator.Rebind();
                animator.Update(0f);
                animator.Play(0, 0, 0f);
            }
        }
    }
}
