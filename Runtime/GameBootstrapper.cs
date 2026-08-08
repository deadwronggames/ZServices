using UnityEngine;
using DeadWrongGames.ZServices.Diagnostics;

namespace DeadWrongGames.ZServices
{
    // this class gets called on game start before loading any scene
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Execute()
        {
            // Instantiate ZServices logging
            LogService.Initialize();
        
            // Instantiate Persistent GameObject with Services
            Object persistentGO = Object.Instantiate(Resources.Load("PF_PersistentGO"));
            persistentGO.name = "PersistentGO";
            Object.DontDestroyOnLoad(persistentGO); 
        } 
    }
}
