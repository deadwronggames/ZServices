using System;
using System.Collections.Generic;
using DeadWrongGames.ZUtils;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

namespace DeadWrongGames.ZServices.Pooling
{
    public class PoolingService : MonoBehaviour, IService
    {
        [SerializeField] BasePoolDefinitionSO[] _poolDefinitions;
        
        // Pools are referenced by Type of the pool object
        // if e.g. normal bullets and explosive bullets should come from the same pool, the configuration of the bullets needs to be done after they are obtained from the pool
        private readonly Dictionary<Type, IObjectPool<Component>> _poolDict = new();
        
        private void Awake()
        {
            foreach (BasePoolDefinitionSO poolDefinition in _poolDefinitions)
                _poolDict.Add(poolDefinition.PoolType, poolDefinition.InstantiatePool());
            
            ServiceLocator.Register(this);
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void OnSceneChanged(Scene _, Scene __) => ClearPools(); 
        private void ClearPools()
        {
            "Clearing Pools()".Log(level: ZMethodsDebug.LogLevel.Info);
            foreach (IObjectPool<Component> pool in _poolDict.Values) pool?.Clear();
        }

        public Poolable<T> Get<T>() where T : Component
        {
            if (!_poolDict.TryGetValue(typeof(T), out IObjectPool<Component> pool))
            {
                $"No pool was defined for {typeof(T)}. Make sure to assign a {nameof(BasePoolDefinitionSO)} instance to this Service. Returning default.".Log(level: ZMethodsDebug.LogLevel.Warning);
                return default;
            }

            return new Poolable<T>((T)pool.Get(), pool);
        }
        
        public struct Poolable<T> where T : Component
        {
            public T Component { get; private set; }
            private readonly IObjectPool<Component> _ownerPool;

            public Poolable(T component, IObjectPool<Component> ownerPool)
            {
                Component = component;
                _ownerPool = ownerPool;
            }

            public void Release()
            {
                if (Component != null && Component && _ownerPool != null) _ownerPool.Release(Component);
                Component = null; // just for safety, e.g. so that component cannot be used further or released again
            }
        }
    }
}