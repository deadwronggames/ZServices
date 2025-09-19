using System;
using System.Collections.Generic;
using DeadWrongGames.ZServices.EventChannel;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, IService> s_serviceDict = new();
        
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => s_serviceDict.Clear();
#endif
        
        public static void Register<TService>(TService serviceToRegister) where TService : IService
        {
            if (s_serviceDict.TryGetValue(typeof(TService), out IService existingService) && existingService != null)
                $"An instance of {typeof(TService)} is already registered as a service. Doing nothing.".Log(level: ZMethodsDebug.LogLevel.Warning);
            else 
                s_serviceDict[typeof(TService)] = serviceToRegister;
        }

        public static bool TryGet<TService>(out TService service) where TService : IService
        {
            service = default;
            
            if (!s_serviceDict.TryGetValue(typeof(TService), out IService serviceObject) || serviceObject == null) return false;
            if (!ZMethods.TryCast(serviceObject, out service, verbose: false)) return false;
            
            return true;
        }
        
        public static TService Get<TService>()
        {
            if (!s_serviceDict.TryGetValue(typeof(TService), out IService serviceObject) || serviceObject == null)
            {
                $"{typeof(TService).Name} requested from ServiceLocator but Service is not registered. Returning default.".Log(level: ZMethodsDebug.LogLevel.Warning);
                return default;
            }
            
            ZMethods.TryCast(serviceObject, out TService service);
            return service;
        }
        
        // provide a MB where e.g. CRs can be run
        private class DummyMB : MonoBehaviour, IService { private void OnDestroy() => s_serviceDict.Remove(typeof(DummyMB)); }
        public static MonoBehaviour GetDummyMB()
        {
            if (TryGet(out DummyMB dummyMB) && dummyMB != null) return dummyMB;
            
            if (!TryGet(out EventBroadcastService eventService)) // TODO change later to GameManager, that seems less arbitrary
            {
                "Not ready to create MB yet. Returning null.".Log(level: ZMethodsDebug.LogLevel.Warning);
                return null;
            }
            
            GameObject dummyGO = new("Dummy GO") { transform = { parent = eventService.transform.parent } };
            dummyMB = dummyGO.AddComponent<DummyMB>();
            Register(dummyMB);

            return dummyMB;
        }
    }
}