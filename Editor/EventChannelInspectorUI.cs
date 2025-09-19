#if UNITY_EDITOR
using DeadWrongGames.ZServices.EventChannel;
using UnityEditor;
using UnityEngine;

namespace DeadWrongGames.ZServices.Editor
{
    [CustomEditor(typeof(EventChannelSO))]
    public class EventChannelInspectorUI : UnityEditor.Editor
    {
        private int    _intToInvoke;
        private float  _floatToInvoke;
        private bool   _boolToInvoke;
        private string _stringToInvoke;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EventChannelSO targetComponent = (EventChannelSO)target;
            
            _intToInvoke = EditorGUILayout.IntField("Int to manually invoke", _intToInvoke);
            _floatToInvoke = EditorGUILayout.FloatField("Float to manually invoke", _floatToInvoke);
            _boolToInvoke = EditorGUILayout.Toggle("Bool to manually invoke", _boolToInvoke);
            _stringToInvoke = EditorGUILayout.TextField("String to manually invoke", _stringToInvoke);
            
            if (GUILayout.Button("Invoke event"))
            {
                GameObject senderEditor = new("Editor script");
                targetComponent.Invoke(senderEditor.transform);
                DestroyImmediate(senderEditor);
            }

            if (GUILayout.Button("Invoke event with int"))
            {
                GameObject senderEditor = new("Editor script");
                targetComponent.Invoke(senderEditor.transform, _intToInvoke);
                DestroyImmediate(senderEditor);
            }
            
            if (GUILayout.Button("Invoke event with float"))
            {
                GameObject senderEditor = new("Editor script");
                targetComponent.Invoke(senderEditor.transform, _floatToInvoke);
                DestroyImmediate(senderEditor);
            }
            
            if (GUILayout.Button("Invoke event with bool"))
            {
                GameObject senderEditor = new("Editor script");
                targetComponent.Invoke(senderEditor.transform, _boolToInvoke);
                DestroyImmediate(senderEditor);
            }
            
            if (GUILayout.Button("Invoke event with string"))
            {
                GameObject senderEditor = new("Editor script");
                targetComponent.Invoke(senderEditor.transform, _stringToInvoke);
                DestroyImmediate(senderEditor);
            }

            if (GUILayout.Button("Print Listeners"))
            {
                targetComponent.PrintListeners();
            }
        }
    }
}
#endif