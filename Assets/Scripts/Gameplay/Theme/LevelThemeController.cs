using System;
using System.Collections.Generic;
using KnockBallsCase.Gameplay.Explosive;
using KnockBallsCase.Gameplay.Glass;
using UnityEngine;
using UnityEngine.UI;

namespace KnockBallsCase.Gameplay.Theme
{
    public sealed class LevelThemeController : MonoBehaviour
    {
        [Serializable]
        private sealed class ThemeDefinition
        {
            public string id = "Theme";
            public Color backgroundColor = new Color(0.12f, 0.12f, 0.12f, 1f);
            public Color platformColor = new Color(0.55f, 0.4f, 0.25f, 1f);
            public Color blockLowColor = new Color(0.75f, 0.45f, 0.3f, 1f);
            public Color blockHighColor = new Color(1f, 0.8f, 0.6f, 1f);
        }

        [Header("Background (Optional)")]
        [SerializeField] private Camera backgroundCamera;
        [SerializeField] private SpriteRenderer backgroundSpriteRenderer;
        [SerializeField] private Image backgroundImage;

        [Header("Themes")]
        [SerializeField] private List<ThemeDefinition> themes = new List<ThemeDefinition>(3);

        [Header("Platform Optimization")]
        [SerializeField] private bool useSharedMaterialForPlatforms = true;
        [SerializeField] private Material platformSharedMaterial;

        [Header("Block Gradient (Shader Driven)")]
        [SerializeField] private bool useShaderDrivenBlockGradient = true;
        [SerializeField] private Material shaderDrivenBlockMaterial;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int ThemeBlockLowColorId = Shader.PropertyToID("_ThemeBlockLowColor");
        private static readonly int ThemeBlockHighColorId = Shader.PropertyToID("_ThemeBlockHighColor");
        private static readonly int ThemeBlockMinYId = Shader.PropertyToID("_ThemeBlockMinY");
        private static readonly int ThemeBlockInvRangeId = Shader.PropertyToID("_ThemeBlockInvRange");

        private readonly List<GameObject> _normalBlocks = new List<GameObject>();
        private MaterialPropertyBlock _mpb;
        private int _currentThemeIndex = -1;

        public int CurrentThemeIndex => _currentThemeIndex;
        public Color UIAccentColor
        {
            get
            {
                // Reading the UI palette must not select a gameplay theme in edit mode.
                var index = _currentThemeIndex >= 0 ? _currentThemeIndex : 0;
                return themes != null && index < themes.Count && themes[index] != null
                    ? Color.Lerp(themes[index].blockHighColor, Color.white, 0.25f)
                    : new Color(1f, 0.68f, 0.25f);
            }
        }

        private void Awake()
        {
            _mpb = new MaterialPropertyBlock();
        }

        private void Reset()
        {
            if (themes.Count > 0)
            {
                return;
            }

            themes = new List<ThemeDefinition>
            {
                new ThemeDefinition
                {
                    id = "Theme A",
                    backgroundColor = new Color(0.08f, 0.1f, 0.18f, 1f),
                    platformColor = new Color(0.38f, 0.3f, 0.22f, 1f),
                    blockLowColor = new Color(0.78f, 0.4f, 0.33f, 1f),
                    blockHighColor = new Color(1f, 0.74f, 0.58f, 1f)
                },
                new ThemeDefinition
                {
                    id = "Theme B",
                    backgroundColor = new Color(0.08f, 0.18f, 0.12f, 1f),
                    platformColor = new Color(0.3f, 0.36f, 0.25f, 1f),
                    blockLowColor = new Color(0.24f, 0.65f, 0.42f, 1f),
                    blockHighColor = new Color(0.62f, 0.92f, 0.66f, 1f)
                },
                new ThemeDefinition
                {
                    id = "Theme C",
                    backgroundColor = new Color(0.2f, 0.12f, 0.08f, 1f),
                    platformColor = new Color(0.45f, 0.3f, 0.18f, 1f),
                    blockLowColor = new Color(0.66f, 0.3f, 0.1f, 1f),
                    blockHighColor = new Color(1f, 0.65f, 0.28f, 1f)
                }
            };
        }

        public void SelectRandomTheme(bool avoidCurrentTheme = true)
        {
            if (themes == null || themes.Count == 0)
            {
                _currentThemeIndex = -1;
                return;
            }

            var nextIndex = UnityEngine.Random.Range(0, themes.Count);
            if (avoidCurrentTheme && themes.Count > 1 && nextIndex == _currentThemeIndex)
            {
                nextIndex = (nextIndex + 1) % themes.Count;
            }

            _currentThemeIndex = nextIndex;
            ApplyBackground();
        }

        public void ApplyCurrentThemeToLevel(IReadOnlyList<GameObject> platformObjects, IReadOnlyList<GameObject> destructibleObjects)
        {
            var theme = GetCurrentTheme();
            if (theme == null)
            {
                return;
            }

            ApplyBackground();
            ApplyPlatformColors(platformObjects, theme.platformColor);
            ApplyDestructibleColors(destructibleObjects, theme);
        }

        private ThemeDefinition GetCurrentTheme()
        {
            if (themes == null || themes.Count == 0)
            {
                return null;
            }

            if (_currentThemeIndex < 0 || _currentThemeIndex >= themes.Count)
            {
                _currentThemeIndex = 0;
            }

            return themes[_currentThemeIndex];
        }

        private void ApplyBackground()
        {
            var theme = GetCurrentTheme();
            if (theme == null)
            {
                return;
            }

            if (backgroundCamera != null)
            {
                backgroundCamera.backgroundColor = theme.backgroundColor;
            }

            if (backgroundSpriteRenderer != null)
            {
                backgroundSpriteRenderer.color = theme.backgroundColor;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = theme.backgroundColor;
            }
        }

        private void ApplyPlatformColors(IReadOnlyList<GameObject> platformObjects, Color platformColor)
        {
            if (platformObjects == null)
            {
                return;
            }

            if (useSharedMaterialForPlatforms)
            {
                ApplyPlatformSharedMaterialColor(platformObjects, platformColor);
                return;
            }

            for (var i = 0; i < platformObjects.Count; i++)
            {
                ApplyColorToObject(platformObjects[i], platformColor);
            }
        }

        private void ApplyPlatformSharedMaterialColor(IReadOnlyList<GameObject> platformObjects, Color platformColor)
        {
            if (platformSharedMaterial != null)
            {
                // Tek shared material üzerinden renk değiştirerek platform call'larını birleştirmeyi hedefleriz.
                if (platformSharedMaterial.HasProperty(BaseColorId))
                {
                    platformSharedMaterial.SetColor(BaseColorId, platformColor);
                }

                if (platformSharedMaterial.HasProperty(ColorId))
                {
                    platformSharedMaterial.SetColor(ColorId, platformColor);
                }
            }

            for (var i = 0; i < platformObjects.Count; i++)
            {
                var obj = platformObjects[i];
                if (obj == null)
                {
                    continue;
                }

                var renderers = obj.GetComponentsInChildren<Renderer>(true);
                for (var r = 0; r < renderers.Length; r++)
                {
                    var renderer = renderers[r];
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (platformSharedMaterial != null)
                    {
                        renderer.sharedMaterial = platformSharedMaterial;
                    }

                    renderer.SetPropertyBlock(null);
                }
            }
        }

        private void ApplyDestructibleColors(IReadOnlyList<GameObject> destructibleObjects, ThemeDefinition theme)
        {
            _normalBlocks.Clear();
            if (destructibleObjects == null)
            {
                return;
            }

            for (var i = 0; i < destructibleObjects.Count; i++)
            {
                var obj = destructibleObjects[i];
                if (obj == null)
                {
                    continue;
                }

                if (obj.GetComponent<GlassBlockFractureOnHit>() != null)
                {
                    continue;
                }

                if (obj.GetComponent<ExplosiveBarrelBlock>() != null)
                {
                    continue;
                }

                _normalBlocks.Add(obj);
            }

            if (_normalBlocks.Count == 0)
            {
                return;
            }

            var minY = float.MaxValue;
            var maxY = float.MinValue;
            for (var i = 0; i < _normalBlocks.Count; i++)
            {
                var y = _normalBlocks[i].transform.position.y;
                if (y < minY)
                {
                    minY = y;
                }

                if (y > maxY)
                {
                    maxY = y;
                }
            }

            var range = Mathf.Max(0.0001f, maxY - minY);
            if (useShaderDrivenBlockGradient)
            {
                ApplyShaderDrivenGradientSetup(theme, minY, range);
                return;
            }

            for (var i = 0; i < _normalBlocks.Count; i++)
            {
                var obj = _normalBlocks[i];
                var normalized = Mathf.Clamp01((obj.transform.position.y - minY) / range);

                var color = Color.Lerp(theme.blockLowColor, theme.blockHighColor, normalized);

                ApplyColorToObject(obj, color);
            }
        }

        private void ApplyShaderDrivenGradientSetup(ThemeDefinition theme, float minY, float range)
        {
            var invRange = 1f / Mathf.Max(0.0001f, range);

            if (shaderDrivenBlockMaterial != null)
            {
                // Gradient parametrelerini material seviyesinde güncelliyoruz; tema değişiminde anında yansır.
                shaderDrivenBlockMaterial.SetColor(ThemeBlockLowColorId, theme.blockLowColor);
                shaderDrivenBlockMaterial.SetColor(ThemeBlockHighColorId, theme.blockHighColor);
                shaderDrivenBlockMaterial.SetFloat(ThemeBlockMinYId, minY);
                shaderDrivenBlockMaterial.SetFloat(ThemeBlockInvRangeId, invRange);
            }
            else
            {
                Shader.SetGlobalColor(ThemeBlockLowColorId, theme.blockLowColor);
                Shader.SetGlobalColor(ThemeBlockHighColorId, theme.blockHighColor);
                Shader.SetGlobalFloat(ThemeBlockMinYId, minY);
                Shader.SetGlobalFloat(ThemeBlockInvRangeId, invRange);
            }

            for (var i = 0; i < _normalBlocks.Count; i++)
            {
                var obj = _normalBlocks[i];
                if (obj == null)
                {
                    continue;
                }

                var renderers = obj.GetComponentsInChildren<Renderer>(true);
                for (var r = 0; r < renderers.Length; r++)
                {
                    var renderer = renderers[r];
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (shaderDrivenBlockMaterial != null)
                    {
                        renderer.sharedMaterial = shaderDrivenBlockMaterial;
                    }

                    renderer.SetPropertyBlock(null);
                }
            }
        }

        private void ApplyColorToObject(GameObject targetObject, Color color)
        {
            if (targetObject == null)
            {
                return;
            }

            var renderers = targetObject.GetComponentsInChildren<Renderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                ApplyColorToRenderer(renderers[i], color);
            }
        }

        private void ApplyColorToRenderer(Renderer renderer, Color color)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorId, color);
            _mpb.SetColor(ColorId, color);
            renderer.SetPropertyBlock(_mpb);
        }

    }
}
