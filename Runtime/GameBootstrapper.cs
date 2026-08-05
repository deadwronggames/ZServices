using UnityEngine;
using DeadWrongGames.ZServices.Debug;

namespace DeadWrongGames.ZServices
{
    // this class gets called on game start before loading any scene
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Execute()
        {
            // Instantiate Persistent GameObject with Services
            Object persistentGO = Object.Instantiate(Resources.Load("PF_PersistentGO"));
            persistentGO.name = "PersistentGO";
            Object.DontDestroyOnLoad(persistentGO); 
            
            // Instantiate Logger
            Logger.Initialize();
        } 
    }
}