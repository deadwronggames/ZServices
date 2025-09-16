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
       
        // Deriving classes define their Component type in the ComponentFactory override
        public Type PoolType => ComponentFactory(_prefab).GetType();
        protected abstract Func<GameObject, Component> ComponentFactory { get; }

        private void OnValidate()
        {
            Component component = ComponentFactory(_prefab);
            if (_prefab != null && component == null)
                $"Prefab {_prefab.name} does not have an {component.GetType()} component.".Log(level: ZMethodsDebug.LogLevel.Warning);
        }

        public IObjectPool<Component> InstantiatePool()
        {
            return new ObjectPool<Component>(
                createFunc: CreateFunc,
                actionOnGet: ActionOnGet,
                actionOnRelease: ActionOnRelease,
                actionOnDestroy: ActionOnDestroy,
                maxSize: (_maxPoolSize > 0) ? _maxPoolSize : int.MaxValue
            );
        }

        protected virtual Component CreateFunc()
        {
            Component instance = ComponentFactory(Instantiate(_prefab));
            instance.gameObject.SetActive(false);
            return instance;
        }
        protected virtual void ActionOnGet(Component poolable)
        {
            poolable.gameObject.SetActive(true);
        }

        protected virtual void ActionOnRelease(Component poolable)
        {
            poolable.gameObject.SetActive(false);
        }

        protected virtual void ActionOnDestroy(Component poolable)
        {
            // When pool is cleared or destroyed, all objects in the pool are destroyed as well
            if (poolable != null) Destroy(poolable.gameObject);
        }
    }
}