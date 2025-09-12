using System;
using System.Collections.Generic;
using System.Linq;
using DeadWrongGames.ZServices.EventChannels;
using DeadWrongGames.ZUtils;
using UnityEngine;

namespace DeadWrongGames.ZServices
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> s_serviceDict = new()
        {
            {typeof(EventService), null},
            // Add more as needed
        };

        
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Type[] serviceKeys = s_serviceDict.Keys.ToArray();
            foreach (Type key in serviceKeys) s_serviceDict[key] = null;
        }
#endif
        
        public static bool IsAllServicesAvailable() => s_serviceDict.All(entry => entry.Value != null);

        public static void Register<TService>(TService serviceToRegister)
        {
            if (s_serviceDict.TryGetValue(typeof(TService), out object existingService))
            {
                if (existingService != null) $"An instance of {typeof(TService)} is already registered as a service. Doing nothing.".Log(level: ZMethodsDebug.LogLevel.Warning);
                else s_serviceDict[typeof(TService)] = serviceToRegister;
            }
            else $"Type {typeof(TService)} is not recognised as a service and could not be registered. Doing Nothing.".Log(level: ZMethodsDebug.LogLevel.Warning);
        }

        public static bool TryGet<TService>(out TService service)
        {
            service = default;
            
            if (!s_serviceDict.TryGetValue(typeof(TService), out object serviceObject)) return false;
            if (!ZMethods.TryCast(serviceObject, out service, verbose: false)) return false;
            
            return true;
        }
        
        public static TService Get<TService>()
        {
            if (!s_serviceDict.TryGetValue(typeof(TService), out object serviceObject))
            {
                $"{typeof(TService).Name} requested from ServiceLocator but Service is not registered. Returning default.".Log(level: ZMethodsDebug.LogLevel.Warning);
                return default;
            }
            
            ZMethods.TryCast(serviceObject, out TService service);
            return service;
        }
        
        // provide a MB where e.g. CRs can be run
        private static MonoBehaviour s_dummyMB;
        private class DummyMB : MonoBehaviour { private void OnDestroy() => s_dummyMB = null; }
        public static MonoBehaviour GetDummyMB()
        {
            if (!TryGet(out EventService eventService)) // TODO change later to GameManager, that seems less arbitrary
            {
                "Not ready to create MB yet. Returning null.".Log(level: ZMethodsDebug.LogLevel.Warning);
                return null;
            }
            
            if (s_dummyMB == null)
            {
                GameObject dummyGO = new("Dummy GO") { transform = { parent = eventService.transform.parent } };
                DummyMB dummy = dummyGO.AddComponent<DummyMB>();
                s_dummyMB = dummy;
            }

            return s_dummyMB;
        }
    }
}