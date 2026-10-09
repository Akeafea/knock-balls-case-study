using UnityEngine;

namespace KnockBallsCase.Core.Pooling
{
    public interface IObjectPool
    {
        GameObject Rent();
        void Return(GameObject instance);
    }
}
