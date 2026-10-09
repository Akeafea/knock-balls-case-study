using KnockBallsCase.Core.Pooling;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Explosive
{
    public sealed class ExplosionEffectPoolProvider : MonoBehaviour
    {
        [SerializeField] private GameObjectPool explosionEffectPool;

        private static ExplosionEffectPoolProvider _instance;

        public static GameObjectPool Pool
        {
            get
            {
                if (_instance == null)
                {
                    var providers = FindObjectsByType<ExplosionEffectPoolProvider>(FindObjectsInactive.Include);
                    if (providers.Length > 0)
                    {
                        _instance = providers[0];
                    }
                }

                return _instance != null ? _instance.explosionEffectPool : null;
            }
        }

        private void Awake()
        {
            _instance = this;

            if (explosionEffectPool == null)
            {
                Debug.LogWarning("ExplosionEffectPoolProvider exists but explosionEffectPool is not assigned.", this);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
