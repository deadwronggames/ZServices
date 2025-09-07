using UnityEngine;

namespace DeadWrongGames.ZServices
{
    // this class gets called on game start before loading any scene
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Execute()
        {
            Object.DontDestroyOnLoad(Object.Instantiate(Resources.Load("PF_PersistentGO"))); 
        } 
    }
}