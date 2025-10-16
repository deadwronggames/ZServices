using System;
using System.Collections.Concurrent;
using DeadWrongGames.ZServices.Time;
using UnityEngine;

namespace DeadWrongGames.ZServices.Task
{
    public class MainThreadDispatcher : MonoBehaviour, IService // , IUpdatable
    {
        private readonly ConcurrentQueue<Action> _executionQueue = new();

        private void Awake()
        {
            ServiceLocator.Register(this);
        }

        // private void Start()
        // {
        //     // Register in start, just to make sure that service is available 
        //     ServiceLocator.Get<UpdateCallbackService>().Register(this);
        // }
        // 
        // private void OnDestroy()
        // {
        //     if(ServiceLocator.TryGet(out UpdateCallbackService service)) service.Unregister(this);
        // }

        // public void OnUpdate()
        // {
        //     // while (_executionQueue.TryDequeue(out Action action))
        //     //     action?.Invoke();
        // }
        
        public void Update()
        {
            while (_executionQueue.TryDequeue(out Action action))
                action?.Invoke();
        }
        
        public static void Enqueue(Action action)
            => ServiceLocator.Get<MainThreadDispatcher>()._executionQueue.Enqueue(action);
    }
}