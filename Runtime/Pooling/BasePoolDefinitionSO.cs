using System;
using DeadWrongGames.ZServices.Diagnostics;
using UnityEngine;
using UnityEngine.Pool;

namespace DeadWrongGames.ZServices.Pooling
{
    /// <summary>
    /// Base ScriptableObject defining how a specific component type is pooled.
    /// Derived classes must implement ComponentFactory to select the pooled component.
    /// </summary>
    public abstract class BasePoolDefinitionSO : ScriptableObject
    {
        [SerializeField] protected GameObject _prefab;
        [SerializeField] int _maxPoolSize = -1;
        
        public Type PoolType => ComponentFactory(_prefab).GetType(); // Type of the pooled component, determined by the factory
        protected abstract Func<GameObject, Component> ComponentFactory { get; } // Defines how to extract the pooled component from the prefab

        private void OnValidate()
        {
            // Validate that prefab contains the required component type
            Component component = ComponentFactory(_prefab);
            if (_prefab != null && component == null)
                LogService.Warning(BuiltInLogCategories.ZSystems, $"Prefab {_prefab.name} does not have an {component.GetType()} component.").Log();
        }

        /// <summary>
        /// Creates a Unity ObjectPool with default create/release/destroy actions.
        /// </summary>
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

        /// <summary>
        /// Instantiates a new prefab instance, gets its component, and disables it.
        /// </summary>
        protected virtual Component CreateFunc()
        {
            Component instance = ComponentFactory(Instantiate(_prefab));
            instance.gameObject.SetActive(false);
            return instance;
        }
        
        /// <summary>
        /// Called when an object is retrieved from the pool.
        /// </summary>
        protected virtual void ActionOnGet(Component poolable)
        {
            poolable.gameObject.SetActive(true);
        }

        /// <summary>
        /// Called when an object is released back into the pool.
        /// </summary>
        protected virtual void ActionOnRelease(Component poolable)
        {
            poolable.gameObject.SetActive(false);
        }

        /// <summary>
        /// Called when an object is destroyed or the pool is cleared.
        /// </summary>
        protected virtual void ActionOnDestroy(Component poolable)
        {
            // When pool is cleared or destroyed, all objects in the pool are destroyed as well
            if (poolable != null) Destroy(poolable.gameObject);
        }
    }
}