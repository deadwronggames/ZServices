using System;
using DeadWrongGames.ZUtils;
using UnityEngine;
using UnityEngine.Pool;

namespace DeadWrongGames.ZServices.Pooling
{
    public abstract class BasePoolDefinitionSO : ScriptableObject
    {
        [SerializeField] protected GameObject _prefab;
        [SerializeField] int _maxPoolSize = -1;

        private void OnValidate()
        {
            if (_prefab != null && _prefab.GetComponent<IPoolable>() == null)
                $"Prefab {_prefab.name} does not have an {nameof(IPoolable)} component.".Log(level: ZMethodsDebug.LogLevel.Warning);
        }

        public Type GetPoolType()
        {
            return _prefab.GetComponent<IPoolable>().GetType();
        }

        public IObjectPool<IPoolable> InstantiatePool()
        {
            return new ObjectPool<IPoolable>(
                createFunc: CreateFunc,
                actionOnGet: ActionOnGet,
                actionOnRelease: ActionOnRelease,
                actionOnDestroy: ActionOnDestroy,
                maxSize: (_maxPoolSize > 0) ? _maxPoolSize : int.MaxValue
            );
        }

        protected abstract IPoolable CreateFunc();
        protected abstract void ActionOnGet(IPoolable poolable);
        protected abstract void ActionOnRelease(IPoolable poolable);
        protected abstract void ActionOnDestroy(IPoolable poolable);
    }
}