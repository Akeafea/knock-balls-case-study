using System;
using System.Collections.Generic;
using KnockBallsCase.Gameplay.Theme;
using KnockBallsCase.UI.Level;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KnockBallsCase.UI.Theme
{
    /// <summary>Serialized originals make the legacy appearance recoverable after saving and reloading.</summary>
    [ExecuteAlways, DefaultExecutionOrder(100)]
    public sealed class GameUIThemeController : MonoBehaviour
    {
        public enum Appearance { Legacy, Themed }

        [Serializable]
        private sealed class GraphicOriginal
        {
            public Graphic graphic;
            public Color color;
            public bool raycastTarget;
            public Sprite sprite;
            public Image.Type imageType;
            public float fontSize;
            public FontStyles fontStyle;
            public float characterSpacing;
        }

        [Serializable]
        private sealed class LayoutOriginal
        {
            public RectTransform rect;
            public Vector2 anchorMin, anchorMax, position, size;
            public int siblingIndex;
        }

        [SerializeField] private Appearance appearance = Appearance.Themed;
        [SerializeField] private LevelThemeController levelTheme;
        [SerializeField, HideInInspector] private List<GraphicOriginal> originals = new List<GraphicOriginal>();
        [SerializeField, HideInInspector] private List<LayoutOriginal> layouts = new List<LayoutOriginal>();
        [SerializeField] private RectTransform playButton, playLabel, title, bestScore;
        [SerializeField] private MiniLevelProgressDisplay progressDisplay;
        [SerializeField] private CanvasScaler loseCanvasScaler;
        [SerializeField, HideInInspector] private CanvasScaler.ScaleMode originalLoseScaleMode;
        [SerializeField, HideInInspector] private Vector2 originalLoseResolution = new Vector2(800f, 600f);
        [SerializeField, HideInInspector] private float originalLoseMatch;
        private Appearance _applied;
        private Color _accent;
        private bool _refreshPending;
        private Sprite _roundedSprite;
        private Texture2D _roundedTexture;

        public Appearance CurrentAppearance => appearance;

        private void OnEnable() { ApplyAppearance(); }
        private void OnDisable() { RestoreOriginals(); }
        private void OnDestroy()
        {
            if (_roundedSprite != null) DestroyImmediate(_roundedSprite);
            if (_roundedTexture != null) DestroyImmediate(_roundedTexture);
        }
        // Unity can invoke OnValidate outside the main thread; defer UI mutations.
        private void OnValidate() { _refreshPending = true; }
        private void Update()
        {
            var accent = GetAccent();
            if (_refreshPending || _applied != appearance || _accent != accent)
            {
                _refreshPending = false;
                ApplyAppearance();
            }
        }

        public void SetAppearance(Appearance value)
        {
            appearance = value;
            ApplyAppearance();
        }

        public void ApplyAppearance()
        {
            RestoreOriginals();
            _applied = appearance;
            _accent = GetAccent();
            if (appearance == Appearance.Legacy) return;
            if (loseCanvasScaler != null)
            {
                loseCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                loseCanvasScaler.referenceResolution = new Vector2(1080f, 1920f);
                loseCanvasScaler.matchWidthOrHeight = 0.5f;
            }

            var surface = new Color(0.045f, 0.06f, 0.09f, 0.96f);
            var ink = new Color(0.95f, 0.97f, 1f);
            foreach (var entry in originals)
            {
                if (entry.graphic == null) continue;
                var name = entry.graphic.gameObject.name;
                var text = entry.graphic as TMP_Text;
                if (text != null)
                {
                    text.color = ink;
                    text.raycastTarget = false;
                    text.characterSpacing = 2f;
                    text.fontSize = Mathf.Min(entry.fontSize, 108f);
                    if (name == "KnockBallsText")
                    {
                        text.color = _accent;
                        text.fontStyle = FontStyles.Bold;
                        text.fontSize = 104f;
                        text.characterSpacing = 5f;
                    }
                    else if (name == "BestScoreText")
                    {
                        text.color = new Color(0.68f, 0.75f, 0.83f);
                        text.fontSize = 48f;
                    }
                    else if (name == "PlayText")
                    {
                        text.color = surface;
                        text.fontStyle = FontStyles.Bold;
                        text.fontSize = 58f;
                    }
                    else if (name == "ContinueText") text.fontSize = 54f;
                    else if (name == "LevelText") text.fontSize = 48f;
                    else if (name == "BallCountText" || name == "ScoreText") text.color = _accent;
                    else if (name == "Cleared Text") text.color = _accent;
                    if (text.transform.parent != null &&
                        (text.transform.parent.name == "CurrentLevel" ||
                         text.transform.parent.name == "RestartButton" ||
                         text.transform.parent.name == "ResetButton")) text.color = surface;
                    continue;
                }
                if (name == "BackDark" || name == "DefaultBack" || name == "LoseAnim")
                    entry.graphic.color = surface;
                else if (name == "Tap To PlayButton") entry.graphic.color = _accent;
                else if (name == "CurrentLevel" || name == "ProgressRig") entry.graphic.color = _accent;
                else if (name == "NextLevel") entry.graphic.color = new Color(0.2f, 0.25f, 0.32f);
                else if (name == "RestartButton" || name == "ResetButton") entry.graphic.color = _accent;
                else if (name == "SoundButton")
                    entry.graphic.color = new Color(0.18f, 0.23f, 0.3f);
                if (entry.graphic is Image image &&
                    (name == "Tap To PlayButton" || name == "CurrentLevel" || name == "NextLevel" ||
                     name.StartsWith("Minilevel1", StringComparison.Ordinal) ||
                     name == "RestartButton" || name == "ResetButton"))
                {
                    image.sprite = GetRoundedSprite();
                    image.type = Image.Type.Sliced;
                }
            }

            Place(title, new Vector2(0.5f, 0.76f), Vector2.zero, new Vector2(920f, 180f));
            Place(bestScore, new Vector2(0.5f, 0.66f), Vector2.zero, new Vector2(800f, 100f));
            Place(playButton, new Vector2(0.5f, 0.24f), Vector2.zero, new Vector2(680f, 150f));
            Place(playLabel, new Vector2(0.5f, 0.24f), Vector2.zero, new Vector2(650f, 130f));
            // The original label is a sibling, so draw the button surface behind it.
            if (playButton != null && playLabel != null) playButton.SetSiblingIndex(playLabel.GetSiblingIndex());
            if (progressDisplay != null)
                progressDisplay.SetColors(Color.Lerp(_accent, Color.white, 0.25f), _accent,
                    new Color(0.2f, 0.25f, 0.32f));
        }

        private Color GetAccent()
        {
            return levelTheme != null ? levelTheme.UIAccentColor : new Color(1f, 0.68f, 0.25f);
        }

        private void RestoreOriginals()
        {
            foreach (var entry in originals)
            {
                if (entry.graphic == null) continue;
                entry.graphic.color = entry.color;
                entry.graphic.raycastTarget = entry.raycastTarget;
                if (entry.graphic is Image image)
                {
                    image.sprite = entry.sprite;
                    image.type = entry.imageType;
                }
                if (entry.graphic is TMP_Text text)
                {
                    text.fontSize = entry.fontSize;
                    text.fontStyle = entry.fontStyle;
                    text.characterSpacing = entry.characterSpacing;
                }
            }
            foreach (var entry in layouts)
            {
                if (entry.rect == null) continue;
                entry.rect.anchorMin = entry.anchorMin;
                entry.rect.anchorMax = entry.anchorMax;
                entry.rect.anchoredPosition = entry.position;
                entry.rect.sizeDelta = entry.size;
                entry.rect.SetSiblingIndex(entry.siblingIndex);
            }
            if (progressDisplay != null) progressDisplay.RestoreColors();
            if (loseCanvasScaler != null)
            {
                loseCanvasScaler.uiScaleMode = originalLoseScaleMode;
                loseCanvasScaler.referenceResolution = originalLoseResolution;
                loseCanvasScaler.matchWidthOrHeight = originalLoseMatch;
            }
        }

        private static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            if (rect == null) return;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private Sprite GetRoundedSprite()
        {
            if (_roundedSprite != null) return _roundedSprite;
            const int side = 64;
            const float radius = 16f;
            _roundedTexture = new Texture2D(side, side, TextureFormat.RGBA32, false)
            {
                name = "UI rounded surface", hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear
            };
            var pixels = new Color32[side * side];
            for (var y = 0; y < side; y++)
            for (var x = 0; x < side; x++)
            {
                var dx = Mathf.Max(Mathf.Abs(x + 0.5f - side / 2f) - (side / 2f - radius), 0f);
                var dy = Mathf.Max(Mathf.Abs(y + 0.5f - side / 2f) - (side / 2f - radius), 0f);
                var alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy));
                pixels[y * side + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
            }
            _roundedTexture.SetPixels32(pixels);
            _roundedTexture.Apply(false, true);
            _roundedSprite = Sprite.Create(_roundedTexture, new Rect(0, 0, side, side),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
            _roundedSprite.name = "UI rounded surface";
            _roundedSprite.hideFlags = HideFlags.HideAndDontSave;
            return _roundedSprite;
        }
    }
}
