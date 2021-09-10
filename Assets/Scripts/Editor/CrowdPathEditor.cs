using System;
using CORE;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor
{
    [CustomEditor(typeof(CrowdPath))]
    public class CrowdPathEditor : UnityEditor.Editor
    {
        private CrowdPath path;
        
        public override void OnInspectorGUI()
        {
            path = (CrowdPath) target;
            
            if (path.pathNodes.Count <= 0)
                path.pathNodes.Add(Vector3.zero);

            path.pathNodes[0] = path.transform.position;

            path.pathChance = EditorGUILayout.Slider("Path Chance: ", path.pathChance, 0.0f, 1.0f);
            
            if (GUILayout.Button("Add Point to Path"))
            {
                path.pathNodes.Add(path.pathNodes[path.pathNodes.Count - 1]);
            }
            if (GUILayout.Button("Remove Last Point"))
            {
                path.pathNodes.RemoveAt(path.pathNodes.Count - 1);
            }
            if (GUILayout.Button("Randomize Arrow Color"))
            {
                path.gizmoCol = new Color(Random.value, Random.value, Random.value, 1.0f);
            }

            path.gizmoCol = EditorGUILayout.ColorField(path.gizmoCol);
            
            
            EditorGUILayout.BeginVertical("HelpBox");
            
            for (int i = 0; i < path.pathNodes.Count; i++)
            {
                GUILayout.Label($"Node {i}: {path.pathNodes[i]}");
            }
            
            EditorGUILayout.EndVertical();

        }

        private void OnSceneGUI()
        {
            path = (CrowdPath) target;

            for (int i = 0; i < path.pathNodes.Count; i++)
            {
                path.pathNodes[i] = Handles.PositionHandle(path.pathNodes[i], Quaternion.identity);
            }
        }
    }
}
