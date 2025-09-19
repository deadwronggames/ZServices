using System;
using System.Collections.Concurrent;
using UnityEngine;

namespace DeadWrongGames.ZServices.Task
{
    public class MainThreadDispatcher : MonoBehaviour, IService
    {
        private readonly ConcurrentQueue<Action> _executionQueue = new();

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        private void Update()
        {
            while (_executionQueue.TryDequeue(out Action action))
                action?.Invoke();
        }
        
        public static void Enqueue(Action action)
            => ServiceLocator.Get<MainThreadDispatcher>()._executionQueue.Enqueue(action);
    }
}