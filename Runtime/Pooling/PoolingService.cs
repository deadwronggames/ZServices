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
        [SerializeField] BasePoolDefinitionSO[] _pools;
        
        // Pools are referenced by Type of the pool object
        // if e.g. normal bullets and explosive bullets should come from the same pool, the configuration of the bullets needs to be done after they are obtained from the pool
        private readonly Dictionary<Type, IObjectPool<IPoolable>> _poolDict = new();
        
        private void Awake()
        {
            foreach (BasePoolDefinitionSO pool in _pools)
                _poolDict.Add(pool.GetPoolType(), pool.InstantiatePool());
            
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
            foreach (IObjectPool<IPoolable> pool in _poolDict.Values) pool?.Clear();
        }

        public TPoolable Get<TPoolable>() where TPoolable : IPoolable
        {
            if (!_poolDict.TryGetValue(typeof(TPoolable), out IObjectPool<IPoolable> pool))
            {
                $"No pool was defined for {typeof(TPoolable)}. Make sure to assign a {nameof(BasePoolDefinitionSO)} instance to this Service. Returning default.".Log(level: ZMethodsDebug.LogLevel.Warning);
                return default;
            }

            return (TPoolable)pool.Get();
        }

        public void Release(IPoolable poolable)
        {
            if (!_poolDict.TryGetValue(poolable.GetType(), out IObjectPool<IPoolable> pool))
            {
                $"No pool available for {poolable.GetType()}. Destroying object.".Log(level: ZMethodsDebug.LogLevel.Warning);
                Destroy(poolable.GameObject);
                return;
            }

            pool.Release(poolable);
        }
    }
}