using DeadWrongGames.ZServices.EventChannels;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DeadWrongGames.ZServices.Editor
{
    public abstract class CreateEventChannel
    {
        [MenuItem("Create/EventChannel")] private static void Create() => CreateScriptableObjectInstance<EventChannel>(Path.Combine(EventService.RESOURCE_FOLDER_PATH, EventService.EVENT_CHANNEL_ASSET_FOLDER_NAME));
        
        private static void CreateScriptableObjectInstance<T>(string folderPath) where T : ScriptableObject
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            
            T instance = ScriptableObject.CreateInstance<T>();
            
            string assetName = typeof(T).Name;
            string fullPath = AssetDatabase.GenerateUniqueAssetPath($"{Path.Combine(folderPath, assetName)}.asset");

            AssetDatabase.CreateAsset(instance, fullPath);
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = instance;
        }
    }
}