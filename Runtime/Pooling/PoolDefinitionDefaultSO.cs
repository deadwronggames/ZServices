using UnityEngine;

namespace DeadWrongGames.ZServices.Pooling
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Pooling/PoolDefault", fileName = "PoolDefault", order = 0)]
    public class PoolDefinitionDefaultSO : BasePoolDefinitionSO
    {
        protected override IPoolable CreateFunc()
        {
            IPoolable instance = Instantiate(_prefab).GetComponent<IPoolable>();
            instance.GameObject.SetActive(false);

            return instance;
        }

        protected override void ActionOnGet(IPoolable poolable)
        {
            poolable.GameObject.SetActive(true);
        }

        protected override void ActionOnRelease(IPoolable poolable)
        {
            poolable.GameObject.SetActive(false);
        }

        protected override void ActionOnDestroy(IPoolable poolable)
        {
            // When pool is cleared or destroyed, all objects in the pool are destroyed as well
            if (poolable != null) Destroy(poolable.GameObject);
        }
    }
}