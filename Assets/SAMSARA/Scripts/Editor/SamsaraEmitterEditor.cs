using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAMSARA.Scripts.Editor
{
    [CustomEditor(typeof(SamsaraEmitter))]
    public class SamsaraEmitterEditor : UnityEditor.Editor
    {
        private SamsaraEmitter selected;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            selected = (SamsaraEmitter) target;

            string lastString = selected.eventReference;

            selected.eventReference = DrawEventInspector(selected.eventReference);
            
            if (!lastString.Equals(selected.eventReference))
                EditorUtility.SetDirty(selected);
        }

        private string DrawEventInspector(string currentEvent)
        {
            if (SamsaraMaster.Instance == null) { return currentEvent; }
            if (SamsaraMaster.Instance.mixerAsset == null) { return currentEvent; }

            SamsaraMixerAsset currentMixerAsset = SamsaraMaster.Instance.mixerAsset;

            if (currentMixerAsset.events.Count == 0) { return currentEvent; }
            
            List<string> eventsRefs = new List<string>();
            int selectedIndex = 0;
            for (int i = 0; i < currentMixerAsset.events.Count; i++)
            {
                string current = currentMixerAsset.events[i].reference;
                eventsRefs.Add(current);
                
                if (current.Equals(currentEvent))
                    selectedIndex = i;
            }

            selectedIndex = EditorGUILayout.Popup("Event", selectedIndex, eventsRefs.ToArray());

            currentMixerAsset.events[selectedIndex] = PrintEventProperties(currentMixerAsset.events[selectedIndex]);

            return eventsRefs[selectedIndex];
        }

        private SamsaraSoundStruct PrintEventProperties(SamsaraSoundStruct soundStruct)
        {
            EditorGUILayout.Space(15);
            
            GUILayout.Label($"Event Properties - clip: [{soundStruct.audioClip.name}]");
            
            EditorGUILayout.BeginVertical("HelpBox");
            
            soundStruct.loop = EditorGUILayout.Toggle("Loop Sound", soundStruct.loop);
            soundStruct.volume = EditorGUILayout.Slider("Volume", soundStruct.volume, 0, 1);
            soundStruct.delay = EditorGUILayout.FloatField("Trigger Delay", soundStruct.delay);
            soundStruct.pitch = EditorGUILayout.Slider("Pitch", soundStruct.pitch, -3, 3);
            soundStruct.randomPitchOffset = EditorGUILayout.Slider("Random Pitch Offset", soundStruct.randomPitchOffset, 0, 1);
            
            EditorGUILayout.EndVertical();
            
            return soundStruct;
        }
    }
}
