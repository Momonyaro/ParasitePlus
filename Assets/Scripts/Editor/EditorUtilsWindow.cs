using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class EditorUtilsWindow : EditorWindow
    {
        private Vector2 scrollPos = Vector2.zero;
        string removeFromPrefsKey = "";

        [MenuItem("Window/Editor Utils")]
        public static void ShowWindow()
        {
            GetWindow<EditorUtilsWindow>("Editor Utils");
        }


        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUIUtility.labelWidth = 100;

            if (GUILayout.Button("Wipe PlayerPrefs"))
                PlayerPrefs.DeleteAll();

            EditorGUILayout.BeginVertical("HelpBox");
            removeFromPrefsKey = EditorGUILayout.TextField("Key", removeFromPrefsKey);
            if (GUILayout.Button("Delete From PlayerPrefs"))
                PlayerPrefs.DeleteKey(removeFromPrefsKey);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndScrollView();
        }
    }

}