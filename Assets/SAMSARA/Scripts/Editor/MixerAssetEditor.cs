using System;
using System.Collections;
using System.Collections.Generic;
using SAMSARA.Scriptables;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SAMSARA.Editor
{
    public class MixerAssetEditor : EditorWindow
    {
        private Vector2 scrollPos = Vector2.zero;
        private Vector2 scrollPos2 = Vector2.zero;
        private MixerAssetScriptable currentAsset;
        private int selectedEvent = 0;
        private string selectedRuntimeEvent = "";

        //Create a two-split window with a list of all events and a larger space for changing event properties.
        [MenuItem("SAMSARA/Mixer Asset Editor")]
        public static void ShowWindow()
        {
            MixerAssetEditor editor = GetWindow<MixerAssetEditor>("Mixer Asset Editor");
        }

        private void OnGUI()
        {
            if (Application.isPlaying)
            {
                selectedEvent = -1;
                DrawRuntimeMixer();
            }
            else
            {
                DrawStaticMixer();
                selectedRuntimeEvent = "";
            }
        }

        private void DrawRuntimeMixer()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 75;
            
            EditorGUILayout.BeginHorizontal("HelpBox", new []{GUILayout.MinHeight(30), GUILayout.ExpandWidth(true)});
            
            EditorGUIUtility.labelWidth = 100;
            GUILayout.Label("SAMSARA Mixer Asset Editor [RUNNING]", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});
            
            EditorGUIUtility.labelWidth = 75;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, new []{GUILayout.Width(300), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)});
            DrawVolumeGroupList();
            DrawRuntimeEventList();
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Separator();

            scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, new []{GUILayout.ExpandWidth(true), GUILayout.MinWidth(400), GUILayout.MaxWidth(3000), GUILayout.ExpandHeight(true)});
            EditorGUILayout.BeginVertical("HelpBox");
            DrawSelectedEvent();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(currentAsset);
        }

        private void DrawStaticMixer()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUIUtility.labelWidth = 75;
            
            EditorGUILayout.BeginHorizontal("HelpBox", new []{GUILayout.MinHeight(30), GUILayout.ExpandWidth(true)});
            
            EditorGUIUtility.labelWidth = 100;
            GUILayout.Label("SAMSARA Mixer Asset Editor", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});
            currentAsset = (MixerAssetScriptable)EditorGUILayout.ObjectField("Mixer Asset: ", currentAsset, typeof(MixerAssetScriptable), false);
            EditorGUIUtility.labelWidth = 75;
            
            EditorGUILayout.EndHorizontal();
            
            if (currentAsset == null) return;
            
            EditorGUILayout.BeginHorizontal();
            
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, new []{GUILayout.MaxWidth(300), GUILayout.MinWidth(200), GUILayout.ExpandHeight(true)});
            DrawVolumeGroupList();
            DrawAssetEventList();
            EditorGUILayout.EndScrollView();
            
            //EditorGUILayout.Separator();
            
            scrollPos2 = EditorGUILayout.BeginScrollView(scrollPos2, new []{GUILayout.ExpandWidth(true), GUILayout.MinWidth(400), GUILayout.MaxWidth(3000), GUILayout.ExpandHeight(true)});
            EditorGUILayout.BeginVertical("HelpBox");
            DrawSelectedEvent();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.EndHorizontal();
            
            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(currentAsset);
        }

        private void DrawVolumeGroupList()
        {
            if (currentAsset == null) return;
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Volume Groups", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});
            if (GUILayout.Button("+", new []{GUILayout.Width(20)}))
            {
                currentAsset.volumeGroups.Add(new SamsaraVolumeGroup()
                {
                    groupVolume = 1,
                    reference = "_groupRef"
                });
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < currentAsset.volumeGroups.Count; i++)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                currentAsset.volumeGroups[i].reference =
                    EditorGUILayout.TextField(currentAsset.volumeGroups[i].reference);
                if (GUILayout.Button("-", new []{GUILayout.Width(20)}))
                {
                    currentAsset.volumeGroups.RemoveAt(i);
                    return;
                }
                EditorGUILayout.EndHorizontal();
                currentAsset.volumeGroups[i].groupVolume =
                    EditorGUILayout.Slider(currentAsset.volumeGroups[i].groupVolume, 0, 1);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawAssetEventList()
        {
            if (currentAsset == null) return;
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Asset Events", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});
            if (GUILayout.Button("+", new []{GUILayout.Width(20)}))
            {
                currentAsset.audioEvents.Add(new AudioEvent()
                {
                    reference = "_newEvent",
                    trackContainer = new EventTrackContainer()
                    {
                        volume = 1,
                        pitch = 1,
                    }
                });
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < currentAsset.audioEvents.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(currentAsset.audioEvents[i].reference))
                {
                    selectedEvent = i;
                }
                if (GUILayout.Button("-", new []{GUILayout.Width(20)}))
                {
                    currentAsset.audioEvents.RemoveAt(i);
                    return;
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawRuntimeEventList()
        {
            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Asset Events", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});
            var dictionary = Samsara.Instance.eventData;

            foreach (KeyValuePair<string, AudioEvent> item in dictionary)
            {
                if (GUILayout.Button(item.Key))
                {
                    selectedRuntimeEvent = item.Key;
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedEvent()
        {
            if (!Application.isPlaying && (selectedEvent < 0 || selectedEvent >= currentAsset.audioEvents.Count)) return;
            if (Application.isPlaying && selectedRuntimeEvent.Equals("")) return;
            if (currentAsset == null) return;

            AudioEvent current;

            if (Application.isPlaying)
            {
                current = Samsara.Instance.GetAudioEventFromReference(selectedRuntimeEvent, out bool success);
                if (!success) return;
            }
            else
            {
                current = currentAsset.audioEvents[selectedEvent];
            }

            EditorGUIUtility.labelWidth = 100;
            current.reference = EditorGUILayout.TextField("Event Reference: ", current.reference);
            EditorGUIUtility.labelWidth = 75;
            
            EditorGUILayout.Space(30);

            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Event Properties", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});

                EditorGUILayout.BeginHorizontal();

                current.trackContainer.volume = EditorGUILayout.Slider("Volume", current.trackContainer.volume, 0, 1);
                EditorGUIUtility.labelWidth = 125;
                DrawVolumeGroupDropdown(ref current);
                    
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                    EditorGUIUtility.labelWidth = 75;
                    current.trackContainer.pitch = EditorGUILayout.Slider("Pitch: ", current.trackContainer.pitch, -1, 2);
                    current.trackContainer.delay = EditorGUILayout.FloatField("Start Delay: ", current.trackContainer.delay);
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();

                    EditorGUIUtility.labelWidth = 125;
                    current.trackContainer.randomPitchOffset = EditorGUILayout.Slider("Random Pitch Offset: ", current.trackContainer.randomPitchOffset, 0, 1);
                    EditorGUIUtility.labelWidth = 75;
                    current.trackContainer.loop = EditorGUILayout.Toggle("Loop Event: ", current.trackContainer.loop);
                
                EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("HelpBox");

                GUILayout.Label("Event Intro/Outro", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});
                EditorGUILayout.BeginHorizontal();
            
                    current.trackContainer.introClip = (AudioClip)EditorGUILayout.ObjectField("Intro Clip: ", current.trackContainer.introClip, typeof(AudioClip), false);
                    current.trackContainer.outroClip = (AudioClip)EditorGUILayout.ObjectField("Outro Clip: ", current.trackContainer.outroClip, typeof(AudioClip), false);
            
                EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();

            DrawTrackLerpGraph(ref current); // Draw some basic graph, outlining the transitions on the lerpValue.
            
            DrawTrackListing(ref current);
            
            if (Application.isPlaying)
            {
                Samsara.Instance.OverwriteAudioEvent(current, out bool success);
                if (!success) return;
            }
            else if (selectedEvent >= 0 && selectedEvent < currentAsset.audioEvents.Count)
                currentAsset.audioEvents[selectedEvent] = current;
        }

        private void DrawVolumeGroupDropdown(ref AudioEvent current)
        {
            int currentIndex = 0;
            List<string> options = new List<string>();
            for (int i = 0; i < currentAsset.volumeGroups.Count; i++)
            {
                options.Add(currentAsset.volumeGroups[i].reference);
                if (currentAsset.volumeGroups[i].reference.Equals(current.trackContainer.volumeGroupRef))
                    currentIndex = i;
            }

            currentIndex= EditorGUILayout.Popup("Event Volume Group: ", currentIndex, options.ToArray());
            current.trackContainer.volumeGroupRef = options[currentIndex];
        }

        private void DrawTrackListing(ref AudioEvent current)
        {
            EventTrackContainer trackContainer = current.trackContainer;

            EditorGUILayout.BeginVertical("HelpBox");
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Event Track Listing", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});
            if (!Application.isPlaying)
            {
                if (GUILayout.Button("+", new []{GUILayout.Width(20)}))
                {
                    trackContainer.tracks.Add(new TrackData()
                    {
                        AudioClip = null
                    });
                    trackContainer.UpdateTrackContainer();
                }
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < trackContainer.tracks.Count; i++)
            {
                EditorGUILayout.BeginHorizontal("Box");
                EditorGUIUtility.labelWidth = 100;
                trackContainer.tracks[i].AudioClip = (AudioClip)EditorGUILayout.ObjectField($"Track [{i}] Clip: ", trackContainer.tracks[i].AudioClip, typeof(AudioClip), false);
                EditorGUIUtility.labelWidth = 75;
                GUILayout.Label( $"Track Audibility: {(CalculateTrackVolume(i, 1, trackContainer.lerpValue, trackContainer.ratio, trackContainer.trackLerpOverlap) * 100).ToString("F1")}%", new []{GUILayout.Width(200)});
                if (!Application.isPlaying)
                {
                    if (GUILayout.Button("-", new []{GUILayout.Width(20)}))
                    {
                        current.trackContainer.tracks.RemoveAt(i);
                        trackContainer.UpdateTrackContainer();
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();

            current.trackContainer = trackContainer;
        }

        private void DrawTrackLerpGraph(ref AudioEvent current)
        {
            Rect scale = EditorGUILayout.BeginVertical("HelpBox", new []{GUILayout.Height(160), GUILayout.ExpandWidth(true)});
            GUILayout.Label("Event Track Interpolation", new GUIStyle() {fontStyle = FontStyle.Bold, padding = new RectOffset(5, 5, 5, 5), normal = new GUIStyleState() {textColor = Color.white}});

            GUILayout.FlexibleSpace();

            current.trackContainer.lerpValue = EditorGUILayout.Slider("Lerp Value: ", current.trackContainer.lerpValue, 0, 1);
            current.trackContainer.trackLerpOverlap = EditorGUILayout.Slider("Lerp Overlap: ", current.trackContainer.trackLerpOverlap, 0.5f, 1);
            EditorGUILayout.EndVertical();

            Rect padding = new Rect(10, 30, 10, 50);
            
            Handles.BeginGUI();
            
            //For each track, place a point at the ratio * i

            Color start = new Color(0.8f, 0f, 0.05f, 0.5f);
            float lerpOverlap = current.trackContainer.trackLerpOverlap;
            float ratio = current.trackContainer.ratio;
            for (int i = 0; i < current.trackContainer.tracks.Count; i++)
            {
                float lerpX = Mathf.Lerp(scale.x + padding.x, scale.x + scale.width - padding.width, ratio * i);
                float lastLerpX = Mathf.Lerp(scale.x + padding.x, scale.x + scale.width - padding.width, Mathf.Max(ratio * (i - 1), 0));
                lastLerpX = Mathf.Lerp(lerpX, lastLerpX, lerpOverlap);
                float nextLerpX = Mathf.Lerp(scale.x + padding.x, scale.x + scale.width - padding.width, ratio * (i + 1));
                nextLerpX = Mathf.Lerp(lerpX, nextLerpX, lerpOverlap);
                
                Vector2 point = new Vector2(lerpX, scale.y + padding.y);
                Vector2 lastPoint = new Vector2(lastLerpX, scale.y + scale.height - padding.height);
                Vector2 nextPoint = new Vector2(nextLerpX, scale.y + scale.height - padding.height);

                Handles.color = start;
                Handles.DrawAAConvexPolygon(lastPoint, point, nextPoint);
            }
            
            Handles.color = Color.white;
            Handles.DrawAAPolyLine(5, 2, new []
            {
                new Vector3(scale.x + padding.x, scale.y + scale.height - padding.height), 
                new Vector3(scale.x + scale.width - padding.width, scale.y + scale.height - padding.height)
            });
            
            Handles.color = Color.white;
            Handles.DrawAAPolyLine(3, 2, new []
            {
                new Vector3(scale.x + padding.x, scale.y + padding.y), 
                new Vector3(scale.x + scale.width - padding.width, scale.y + padding.y)
            });
            
            float x = Mathf.Lerp(scale.x + padding.x, scale.x + scale.width - padding.width, current.trackContainer.lerpValue);
            float y = scale.y + padding.y;
            Handles.color = new Color(0.25f, 0.64f, 1f);
            Handles.DrawAAPolyLine(4, 2, new []
            {
                new Vector3(x, scale.y + padding.y), 
                new Vector3(x, scale.y + scale.height - padding.height)
            });
            
            Vector2 triPointA = new Vector2(x - 6, y);
            Vector2 triPointB = new Vector2(x + 6, y);
            Vector2 triPointC = new Vector2(x, y + 9);
            
            y = scale.y + scale.height - padding.height;
            Vector2 triPointD = new Vector2(x - 6, y);
            Vector2 triPointE = new Vector2(x + 6, y);
            Vector2 triPointF = new Vector2(x, y - 9);
            
            Handles.DrawAAConvexPolygon(triPointA, triPointB, triPointC);
            Handles.DrawAAConvexPolygon(triPointD, triPointE, triPointF);
            
            Handles.EndGUI();
        }
        
        public float CalculateTrackVolume(int index, float volume, float currentLerp, float lerpRatio, float trackLerpOverlap)
        {
            float ratio = lerpRatio;
            float mult = currentLerp - ratio * index; // Center it around zero
            if (mult > -ratio * trackLerpOverlap && mult < ratio * trackLerpOverlap)
            {
                return volume * (1 - (Mathf.Abs(mult) / (ratio * trackLerpOverlap)));
            }
            else
            {
                return 0;
            }
            
        }
    }
    
    
}
