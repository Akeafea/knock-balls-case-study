using UnityEngine;

namespace KnockBallsCase.Core.Pooling
{
    public sealed class PooledInstance : MonoBehaviour
    {
        private IObjectPool _ownerPool;

        public void SetOwnerPool(IObjectPool ownerPool)
        {
            _ownerPool = ownerPool;
        }

        public void ReturnToPool()
        {
            if (_ownerPool == null)
            {
                gameObject.SetActive(false);
                return;
            }

            _ownerPool.Return(gameObject);
        }
    }
}
