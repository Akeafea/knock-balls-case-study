using KnockBallsCase.Core.Pooling;
using UnityEngine;

namespace KnockBallsCase.Gameplay.Glass
{
    public sealed class FracturedGlassPoolProvider : MonoBehaviour
    {
        [SerializeField] private GameObjectPool fracturedGlassPool;

        private static FracturedGlassPoolProvider _instance;

        public static GameObjectPool Pool
        {
            get
            {
                if (_instance == null)
                {
                    var providers = FindObjectsByType<FracturedGlassPoolProvider>(FindObjectsInactive.Include);
                    if (providers.Length > 0)
                    {
                        _instance = providers[0];
                    }
                }

                return _instance != null ? _instance.fracturedGlassPool : null;
            }
        }

        private void Awake()
        {
            _instance = this;

            if (fracturedGlassPool == null)
            {
                Debug.LogWarning("FracturedGlassPoolProvider exists but fracturedGlassPool is not assigned.", this);
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
