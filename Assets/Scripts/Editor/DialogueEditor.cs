using System.Collections.Generic;
using System.Linq;
using Dialogue;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(DialogueScriptable))]
    public class DialogueEditor : UnityEditor.Editor
    {
        private DialogueScriptable lastInstance = null;
        
        public override void OnInspectorGUI()
        {
            lastInstance = (DialogueScriptable) target;
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUIUtility.labelWidth = 150;

            EditorGUILayout.BeginVertical("HelpBox");
            lastInstance.transitionToOnEof = (DialogueScriptable) 
                EditorGUILayout.ObjectField("Transition on Eof: ", lastInstance.transitionToOnEof, typeof(DialogueScriptable), false);
            EditorGUILayout.BeginHorizontal();
            lastInstance.goToSceneOnEof = EditorGUILayout.Toggle("Go to Scene on Eof:", lastInstance.goToSceneOnEof);
            if (lastInstance.goToSceneOnEof)
            {
                lastInstance.destinationSceneOnEof =
                    EditorGUILayout.TextField("Destination Scene Name: ", lastInstance.destinationSceneOnEof);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            DrawSpeakerEditor();
            
            EditorGUILayout.Space(15);
            
            DrawGroupEditor();
            
            EditorGUILayout.Space(25);
            
            DrawComponents();
            
            EditorGUILayout.Space(100);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawComponents()
        {
            EditorGUILayout.BeginVertical("Box");
            
            if (lastInstance.components.Count > 0)
                if (InsertComponent(0)) return;
                
            for (int i = 0; i < lastInstance.components.Count; i++)
            {
                DialogueComponent current = lastInstance.components[i];
                if (!GetGroupVisible(current.groupRef)) continue;
                EditorGUILayout.Space(45);
                bool deleteFlag = false;
                switch (current.GetComponentType())
                {
                    case ComponentTypes.DIALOGUE_BOX:
                        DrawDefaultProperties(current, out deleteFlag);

                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }
                        
                        break;

                    case ComponentTypes.CHOICE:
                        DrawDefaultProperties(current, out deleteFlag);

                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }

                        break;

                    case ComponentTypes.DESTROY:
                        DrawDefaultProperties(current, out deleteFlag);

                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }
                        
                        break;
                    
                    case ComponentTypes.WAIT:
                        DrawDefaultProperties(current, out deleteFlag);

                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }
                        
                        break;
                    
                    case ComponentTypes.SPAWN_OBJECT:
                        DrawDefaultProperties(current, out deleteFlag);

                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }
                        
                        break;
                    
                    case ComponentTypes.SPAWN_OBJECT_BG:
                        DrawDefaultProperties(current, out deleteFlag);

                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }
                        
                        break;
                    
                    case ComponentTypes.DESTROY_ALL:
                        DrawDefaultProperties(current, out deleteFlag);

                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }
                        
                        break;
                    
                    case ComponentTypes.PLAY_SFX:
                        DrawDefaultProperties(current, out deleteFlag);

                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }

                        break;
                    
                    case ComponentTypes.PLAYER_NAME_BOX:
                        DrawDefaultProperties(current, out deleteFlag);
                        
                        if (deleteFlag)
                        {
                            lastInstance.components.RemoveAt(i);
                            return;
                        }
                        
                        break;
                    
                    default: // Error case, ignore this for now
                        break;
                }
                lastInstance.components[i] = current;
                EditorGUILayout.Space(30);
                if (InsertComponent(i + 1)) return;
            }
            
            if (lastInstance.components.Count == 0)
                if (InsertComponent(0)) return;
            
            EditorGUILayout.EndVertical();
        }

        private void DrawDefaultProperties(DialogueComponent current, out bool deleteThis)
        {
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal("HelpBox");
            current.reveal = EditorGUILayout.Toggle(current.reveal);
            GUILayout.Label(current.GetComponentType().ToString());
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Remove Component", GUILayout.Width(220)))
            {
                deleteThis = true;
                return;
            }
            EditorGUILayout.EndHorizontal();
            if (current.reveal)
            {
                switch (current.GetComponentType())
                {
                    case ComponentTypes.DIALOGUE_BOX:
                        current = DrawDialogueBoxEditor((DialogueBoxComponent) current);
                        break;
                    case ComponentTypes.CHOICE:
                        current = DrawChoiceEditor((ChoiceComponent) current);
                        break;
                    case ComponentTypes.SPAWN_OBJECT:
                        current = DrawSpawnEditor((SpawnObjectComponent) current);
                        break;
                    case ComponentTypes.SPAWN_OBJECT_BG:
                        current = DrawBackgroundSpawnEditor((SpawnBgObjectComponent) current);
                        break;
                    case ComponentTypes.DESTROY:
                        current = DrawDestroyEditor((DestroyComponent) current);
                        break;
                    case ComponentTypes.DESTROY_ALL:
                        current = DrawDestroyAllEditor((DestroyAllComponent) current);
                        break;
                    case ComponentTypes.WAIT:
                        current = DrawWaitEditor((WaitComponent) current);
                        break;
                    case ComponentTypes.PLAYER_NAME_BOX:
                        current = DrawNameBoxEditor((CharNameBoxComponent) current);
                        break;
                    case ComponentTypes.PLAY_SFX:
                        current = DrawPlaySFXEditor((PlaySoundComponent) current);
                        break;
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Component Reference: " + current.reference);
                current.groupRef = GetGroup(current.groupRef);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            deleteThis = false;
        }

        private SpawnObjectComponent DrawSpawnEditor(SpawnObjectComponent spawnObjectComponent)
        {
            spawnObjectComponent.reference = EditorGUILayout.TextField("Component Reference: ", spawnObjectComponent.reference);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This Component will spawn a graphical object (i.e. images) onto a canvas.", MessageType.Info);
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            spawnObjectComponent.objectPrefab =
                (GameObject) EditorGUILayout.ObjectField("Prefab: ", spawnObjectComponent.objectPrefab, typeof(GameObject), false);
            EditorGUIUtility.labelWidth = 80;
            spawnObjectComponent.screenPosition = EditorGUILayout.Vector2Field("Screen Pos: ", spawnObjectComponent.screenPosition);
            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.EndHorizontal();
            
            
            return spawnObjectComponent;
        }
        
        private SpawnBgObjectComponent DrawBackgroundSpawnEditor(SpawnBgObjectComponent spawnBgObjectComponent)
        {
            spawnBgObjectComponent.reference = EditorGUILayout.TextField("Component Reference: ", spawnBgObjectComponent.reference);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This Component will spawn a graphical object (i.e. images) onto a canvas.", MessageType.Info);
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            spawnBgObjectComponent.objectPrefab =
                (GameObject) EditorGUILayout.ObjectField("Prefab: ", spawnBgObjectComponent.objectPrefab, typeof(GameObject), false);
            spawnBgObjectComponent.backgroundImage = 
                (Sprite) EditorGUILayout.ObjectField("Background Img: ", spawnBgObjectComponent.backgroundImage, typeof(Sprite), false);
            EditorGUIUtility.labelWidth = 80;
            spawnBgObjectComponent.screenPosition = EditorGUILayout.Vector2Field("Screen Pos: ", spawnBgObjectComponent.screenPosition);
            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.EndHorizontal();
            
            
            return spawnBgObjectComponent;
        }

        private WaitComponent DrawWaitEditor(WaitComponent waitComponent)
        {
            waitComponent.reference = EditorGUILayout.TextField("Component Reference: ", waitComponent.reference);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This Component will wait for it's timer to run out before passing the torch to the next component.", MessageType.Info);
            EditorGUILayout.Space(10);
            waitComponent.waitTime = EditorGUILayout.FloatField("Timer Length: ", waitComponent.waitTime);

            return waitComponent;
        }

        private CharNameBoxComponent DrawNameBoxEditor(CharNameBoxComponent nameBoxComponent)
        {
            nameBoxComponent.reference = EditorGUILayout.TextField("Component Reference: ", nameBoxComponent.reference);
            EditorGUILayout.BeginHorizontal();
            nameBoxComponent.objectPrefab =
                (GameObject) EditorGUILayout.ObjectField("Prefab: ", nameBoxComponent.objectPrefab, typeof(GameObject), false);
            EditorGUIUtility.labelWidth = 80;
            nameBoxComponent.screenPosition = EditorGUILayout.Vector2Field("Screen Pos: ", nameBoxComponent.screenPosition);
            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This Component will spawn the name box for naming the player character.", MessageType.Info);

            return nameBoxComponent;
        }
        
        private DestroyComponent DrawDestroyEditor(DestroyComponent destroyComponent)
        {
            destroyComponent.reference = EditorGUILayout.TextField("Component Reference: ", destroyComponent.reference);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This Component will destroy the currently instanced graphic of the referenced component", MessageType.Info);
            EditorGUILayout.Space(10);
            destroyComponent.refToDestroy = GetComponentReference(destroyComponent.refToDestroy);

            return destroyComponent;
        }
        
        private PlaySoundComponent DrawPlaySFXEditor(PlaySoundComponent playSoundComponent)
        {
            playSoundComponent.reference = EditorGUILayout.TextField("Component Reference: ", playSoundComponent.reference);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("Using reSamsara, this component will request a sound to be played.", MessageType.Info);
            EditorGUILayout.Space(10);
            playSoundComponent.soundEventRef =
                EditorGUILayout.TextField("Sound Event Reference: ", playSoundComponent.soundEventRef);
            playSoundComponent.playLayered = EditorGUILayout.Toggle("Play Layered: ", playSoundComponent.playLayered);
            if (!playSoundComponent.playLayered)
                playSoundComponent.trackLayer = EditorGUILayout.IntField("Track Layer: ", playSoundComponent.trackLayer);

            return playSoundComponent;
        }
        
        private DestroyAllComponent DrawDestroyAllEditor(DestroyAllComponent destroyComponent)
        {
            destroyComponent.reference = EditorGUILayout.TextField("Component Reference: ", destroyComponent.reference);
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("This Component will destroy all currently instanced graphics", MessageType.Info);
            EditorGUILayout.Space(10);

            return destroyComponent;
        }

        private DialogueBoxComponent DrawDialogueBoxEditor(DialogueBoxComponent boxComponent)
        {
            boxComponent.reference = EditorGUILayout.TextField("Component Reference: ", boxComponent.reference);
            boxComponent.perSentenceWriteToScreen = EditorGUILayout.Toggle("Write per Sentence: ",
                boxComponent.perSentenceWriteToScreen);
            boxComponent.requireContinueInput =
                EditorGUILayout.Toggle("Require Player Input: ", boxComponent.requireContinueInput);
            EditorGUILayout.BeginHorizontal();
            boxComponent.objectPrefab =
                (GameObject) EditorGUILayout.ObjectField("Prefab: ", boxComponent.objectPrefab, typeof(GameObject), false);
            EditorGUIUtility.labelWidth = 80;
            boxComponent.screenPosition = EditorGUILayout.Vector2Field("Screen Pos: ", boxComponent.screenPosition);
            EditorGUIUtility.labelWidth = 150;
            EditorGUILayout.EndHorizontal();
            boxComponent.writingSoundEvent =
                EditorGUILayout.TextField("Writing SFX Event: ", boxComponent.writingSoundEvent);
            
            EditorGUILayout.BeginVertical("HelpBox");

            if (boxComponent.dialogueBoxes.Count > 0)
                if (InsertDialogueBox(boxComponent, 0))
                    return boxComponent;
            
            for (int i = 0; i < boxComponent.dialogueBoxes.Count; i++)
            {
                DialogueBoxComponent.DialogueBox current = boxComponent.dialogueBoxes[i];
                
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginHorizontal();
                current.speakerReference = GetSpeaker(current.speakerReference);
                if (GUILayout.Button("Remove Dialogue Box"))
                {
                    boxComponent.dialogueBoxes.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                current.buildTime = EditorGUILayout.Slider("Build Time: ", current.buildTime, 0, .5f);
                current.text = EditorGUILayout.TextArea(current.text, GUILayout.MinHeight(50));
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);

                boxComponent.dialogueBoxes[i] = current;
                
                if (InsertDialogueBox(boxComponent, i + 1))
                    break;
            }
            
            if (boxComponent.dialogueBoxes.Count == 0)
                if (InsertDialogueBox(boxComponent, 0))
                    return boxComponent;

            EditorGUILayout.EndVertical();
            return boxComponent;
        }

        private ChoiceComponent DrawChoiceEditor(ChoiceComponent boxComponent)
        {
            boxComponent.reference = EditorGUILayout.TextField("Component Reference: ", boxComponent.reference);
            EditorGUILayout.BeginHorizontal();
            boxComponent.objectPrefab =
                (GameObject)EditorGUILayout.ObjectField("Prefab: ", boxComponent.objectPrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginVertical("HelpBox");

            if (boxComponent.choices.Count > 0)
                if (InsertChoice(boxComponent, 0))
                    return boxComponent;

            for (int i = 0; i < boxComponent.choices.Count; i++)
            {
                ChoiceComponent.ChoiceData current = boxComponent.choices[i];

                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Remove Branch"))
                {
                    boxComponent.choices.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                current.text = EditorGUILayout.TextArea(current.text, GUILayout.MinHeight(50));
                current.dialogueBranch = (DialogueScriptable) EditorGUILayout.ObjectField("Transition on Eof: ", current.dialogueBranch, typeof(DialogueScriptable), false);
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(10);

                boxComponent.choices[i] = current;

                if (InsertChoice(boxComponent, i + 1))
                    break;
            }

            if (boxComponent.choices.Count == 0)
                if (InsertChoice(boxComponent, 0))
                    return boxComponent;

            EditorGUILayout.EndVertical();
            return boxComponent;
        }

        private string GetSpeaker(string current)
        {
            int lastSelected = 0;
            string[] currentSpeakers = new string[lastInstance.speakers.Length];
            for (int i = 0; i < currentSpeakers.Length; i++)
            {
                currentSpeakers[i] = lastInstance.speakers[i].speakerReference;
                if (currentSpeakers[i].Equals(current))
                    lastSelected = i;
            }

            return currentSpeakers[EditorGUILayout.Popup("Speaker", lastSelected, currentSpeakers)];
        }
        
        private string GetGroup(string current)
        {
            int lastSelected = 0;

            if (lastInstance.groups.Length == 0)
            {
                lastInstance.groups = new[]
                {
                    new Group("Default", "_default"),
                };
            }
            
            string[] currentGroups = new string[lastInstance.groups.Length];
            
            for (int i = 0; i < currentGroups.Length; i++)
            {
                currentGroups[i] = lastInstance.groups[i].groupReference;
                if (currentGroups[i].Equals(current))
                    lastSelected = i;
            }

            return currentGroups[EditorGUILayout.Popup("Group", lastSelected, currentGroups)];
        }

        private bool GetGroupVisible(string groupRef)
        {
            for (int i = 0; i < lastInstance.groups.Length; i++)
            {
                if (lastInstance.groups[i].groupReference.Equals(groupRef))
                    return lastInstance.groups[i].visible;
            }

            return true;
        }
        
        private string GetComponentReference(string current)
        {
            int lastSelected = 0;
            string[] currentComponents = new string[lastInstance.components.Count];
            for (int i = 0; i < currentComponents.Length; i++)
            {
                currentComponents[i] = lastInstance.components[i].reference;
                if (currentComponents[i].Equals(current))
                    lastSelected = i;
            }

            return currentComponents[EditorGUILayout.Popup("Components", lastSelected, currentComponents)];
        }
        
        private bool InsertDialogueBox(DialogueBoxComponent boxComponent, int index) //Return true if a new component is added
        {
            EditorGUILayout.BeginVertical("Box");

            if (GUILayout.Button("Insert Dialogue Line" , GUILayout.Width(150)))
            {
                boxComponent.dialogueBoxes.Insert(index, new DialogueBoxComponent.DialogueBox()
                {
                    buildTime = 0.0456f
                });
                return true;
            }
            
            EditorGUILayout.EndVertical();
            return false;
        }

        private bool InsertChoice(ChoiceComponent boxComponent, int index) //Return true if a new component is added
        {
            EditorGUILayout.BeginVertical("Box");

            if (GUILayout.Button("Insert Branch", GUILayout.Width(150)))
            {
                boxComponent.choices.Insert(index, new ChoiceComponent.ChoiceData());
                return true;
            }

            EditorGUILayout.EndVertical();
            return false;
        }

        private bool InsertComponent(int index) //Return true if a new component is added
        {
            EditorGUILayout.BeginVertical("HelpBox");
            ComponentTypes type = ComponentTypes.NONE;
            type = (ComponentTypes)EditorGUILayout.EnumPopup("Insert Component: ", type);
            EditorGUILayout.EndVertical();
            
            switch (type)
            {
                case ComponentTypes.DIALOGUE_BOX:
                    DialogueBoxComponent dialogueBoxComponent = new DialogueBoxComponent()
                    {
                        screenPosition = new Vector2(0, -550),
                        perSentenceWriteToScreen = false,
                        writingSoundEvent = "_keyPress",
                        objectPrefab = Resources.Load<GameObject>("DialogueBox")
                    };
                    dialogueBoxComponent.reference = "_diagBox" + index;
                    lastInstance.components.Insert(index, dialogueBoxComponent);
                    EditorUtility.SetDirty(target);
                    
                    return true;

                case ComponentTypes.CHOICE:
                    ChoiceComponent choiceComponent = new ChoiceComponent()
                    {
                        objectPrefab = Resources.Load<GameObject>("ThoughtPrefab")
                    };
                    choiceComponent.reference = "_choice" + index;
                    lastInstance.components.Insert(index, choiceComponent);
                    EditorUtility.SetDirty(target);

                    return true;
                
                case ComponentTypes.DESTROY:
                    DestroyComponent destroyComponent = new DestroyComponent();
                    destroyComponent.reference = "_destroyComp" + index;
                    lastInstance.components.Insert(index, destroyComponent);
                    EditorUtility.SetDirty(target);
                    
                    return true;
                
                case ComponentTypes.WAIT:
                    WaitComponent waitComponent = new WaitComponent();
                    waitComponent.reference = "_waitComp" + index;
                    lastInstance.components.Insert(index, waitComponent);
                    EditorUtility.SetDirty(target);
                    return true;
                
                case ComponentTypes.PLAY_SFX:
                    PlaySoundComponent sfxComponent = new PlaySoundComponent();
                    sfxComponent.reference = "_sfxEventComp" + index;
                    lastInstance.components.Insert(index, sfxComponent);
                    EditorUtility.SetDirty(target);
                    return true;
                
                case ComponentTypes.SPAWN_OBJECT:
                    SpawnObjectComponent spawnObject = new SpawnObjectComponent();
                    spawnObject.reference = "_spawnObjComp" + index;
                    lastInstance.components.Insert(index, spawnObject);
                    EditorUtility.SetDirty(target);
                    return true;
                
                case ComponentTypes.SPAWN_OBJECT_BG:
                    SpawnBgObjectComponent spawnBgObject = new SpawnBgObjectComponent();
                    spawnBgObject.reference = "_spawnBGObjComp" + index;
                    lastInstance.components.Insert(index, spawnBgObject);
                    EditorUtility.SetDirty(target);
                    return true;
                
                case ComponentTypes.DESTROY_ALL:
                    DestroyAllComponent destroyAll = new DestroyAllComponent();
                    destroyAll.reference = "_destroyAll" + index;
                    lastInstance.components.Insert(index, destroyAll);
                    EditorUtility.SetDirty(target);
                    return true;
                
                case ComponentTypes.PLAYER_NAME_BOX:
                    CharNameBoxComponent nameBox = new CharNameBoxComponent();
                    nameBox.reference = "_charNameBox";
                    lastInstance.components.Insert(index, nameBox);
                    return true;

                default:
                    return false;
            }
        }
        
        private void DrawSpeakerEditor()
        {
            GUILayout.Label("Speakers");
            EditorGUILayout.BeginVertical("HelpBox");

            for (int i = 0; i < lastInstance.speakers.Length; i++)
            {
                Speaker current = lastInstance.speakers[i];
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginHorizontal();
                Texture2D tempPhoto = AssetPreview.GetAssetPreview(current.speakerPhoto);
                GUILayout.Label(tempPhoto, GUILayout.Height(60));
                current.speakerPhoto = (Sprite) EditorGUILayout.ObjectField(current.speakerPhoto, typeof(Sprite), false);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                current.speakerName = EditorGUILayout.TextField("Name: ", current.speakerName);
                current.speakerReference = EditorGUILayout.TextField("Reference: ", current.speakerReference);
                EditorGUILayout.EndHorizontal();
                lastInstance.speakers[i] = current;
                if (GUILayout.Button("Remove Speaker: " + current.speakerName))
                {
                    List<Speaker> temp = lastInstance.speakers.ToList();
                    temp.RemoveAt(i);
                    lastInstance.speakers = temp.ToArray();
                    return;
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Speaker", GUILayout.Height(30)))
            {
                List<Speaker> temp = lastInstance.speakers.ToList();
                temp.Add(new Speaker("New Speaker", "_newSpeaker" + (temp.Count + 1), null));
                lastInstance.speakers = temp.ToArray();
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawGroupEditor()
        {
            GUILayout.Label("Groups");
            EditorGUILayout.BeginVertical("HelpBox");
            
            for (int i = 0; i < lastInstance.groups.Length; i++)
            {
                Group current = lastInstance.groups[i];
                EditorGUILayout.BeginVertical("HelpBox");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                current.groupName = EditorGUILayout.TextField("Name: ", current.groupName);
                current.groupReference = EditorGUILayout.TextField("Reference: ", current.groupReference);
                EditorGUILayout.EndHorizontal();
                current.visible = EditorGUILayout.Toggle("Visible in Editor:", current.visible);
                lastInstance.groups[i] = current;
                if (GUILayout.Button("Remove Group: " + current.groupName))
                {
                    List<Group> temp = lastInstance.groups.ToList();
                    temp.RemoveAt(i);
                    lastInstance.groups = temp.ToArray();
                    return;
                }
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("Add Group", GUILayout.Height(30)))
            {
                List<Group> temp = lastInstance.groups.ToList();
                temp.Add(new Group("New Group", "_newGroup" + (temp.Count + 1)));
                lastInstance.groups = temp.ToArray();
            }
            
            EditorGUILayout.EndVertical();
        }
    }
}
