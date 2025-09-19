#if UNITY_EDITOR
using DeadWrongGames.ZServices.EventChannel;
using System.IO;
using DeadWrongGames.ZConstants;
using DeadWrongGames.ZServices.Audio;
using UnityEditor;
using UnityEngine;

namespace DeadWrongGames.ZServices.Editor
{
    public abstract class CreateServiceScriptableObjects
    {
        private static readonly string BASE_PATH = Path.Combine(Constants.ROOT_FOLDER_NAME, Constants.PROJECT_FOLDER_NAME, Constants.SERVICES_FOLDER_NAME, Constants.SERVICES_ASSETS_FOLDER_NAME, "Resources");
        [MenuItem("Create/EventChannelSO")] private static void CreateEventChannelSO() => CreateScriptableObjectInstance<EventChannelSO>(Path.Combine(BASE_PATH, Constants.SERVICES_EVENT_CHANNEL_SO_FOLDER_NAME));
        [MenuItem("Create/SoundDataSO")] private static void CreateSoundDataSO() => CreateScriptableObjectInstance<SoundDataSO>(Path.Combine(BASE_PATH, Constants.SERVICES_SOUND_DATA_SO_FOLDER_NAME));
        
        private static T CreateScriptableObjectInstance<T>(string folderPath) where T : ScriptableObject
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            T instance = ScriptableObject.CreateInstance<T>();
            
            string assetName = typeof(T).Name;
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{Path.Combine(folderPath, assetName)}.asset");

            AssetDatabase.CreateAsset(instance, fullPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = instance;

            return instance;
        }
    }
}
#endif