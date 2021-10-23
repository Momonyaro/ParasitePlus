using Scriptables;
using System.Collections;
using System.Collections.Generic;
using BattleSystem;
using BattleSystem.AI;
using BattleSystem.Interjects;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(EntityScriptable))]
    public class EntityEditor : UnityEditor.Editor
    {
        private EntityScriptable current;

        public override void OnInspectorGUI()
        {

            current = (EntityScriptable) target;
            EditorGUI.BeginChangeCheck();

            base.OnInspectorGUI();

            GUIStyle textStyle = EditorStyles.label;
            textStyle.wordWrap = true;

            EditorGUILayout.Space(30);

            var entityAI = current.GetAIComponent();

            EditorGUILayout.BeginVertical("HelpBox");
            GUILayout.Label("Entity AI Component");

            if (entityAI.personalityNodes.Count == 0)
            {
                entityAI.personalityNodes = new List<EntityAIComponent.PersonalityNode>
                        {
                            new EntityAIComponent.PersonalityNode()
                            {
                                healthPercentLessThan = 1.0f,
                                roundsPassed = 0,
                                moveSelect = entityAI.AllMoveSelect[0],
                                targetComp = entityAI.AllTargeting[0],
                            }
                        };
            }

            if (GUILayout.Button("Add Node"))
            {
                entityAI.personalityNodes.Add(
                    new EntityAIComponent.PersonalityNode()
                    {
                        healthPercentLessThan = 1.0f,
                        roundsPassed = 0,
                        moveSelect = entityAI.AllMoveSelect[0],
                        targetComp = entityAI.AllTargeting[0],
                    });
            }

            for (int i = 0; i < entityAI.personalityNodes.Count; i++)
            {
                EntityAIComponent.PersonalityNode per = entityAI.personalityNodes[i];
                DrawNodeEditor(ref per);
                entityAI.personalityNodes[i] = per;

                if (GUILayout.Button("Delete Node"))
                {
                    entityAI.personalityNodes.RemoveAt(i);
                    break;
                }
            }


            current.OverwriteAIComponent(entityAI);


            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }

            void DrawNodeEditor(ref EntityAIComponent.PersonalityNode personalityNode)
            {
                EditorGUILayout.Space(30);
                EditorGUILayout.BeginVertical("HelpBox");

                GUILayout.Label("Personality requirements");
                EditorGUILayout.BeginVertical("HelpBox");

                personalityNode.healthPercentLessThan = EditorGUILayout.Slider("Health Percent Less Than", personalityNode.healthPercentLessThan, 0, 1);
                personalityNode.roundsPassed = EditorGUILayout.IntField("Rounds Passed", personalityNode.roundsPassed);
                personalityNode.trigger = EditorGUILayout.TextField("Node Trigger", personalityNode.trigger);

                DrawNodeAbilityEditor(ref personalityNode);
                DrawNodeInterjectEditor(ref personalityNode);
                
                EditorGUILayout.EndVertical();

                //Draw move select properties
                DrawNodeMoveSelectEditor(ref personalityNode);
                //Draw targeting comp properties
                DrawNodeTargetingEditor(ref personalityNode);

                EditorGUILayout.EndVertical();
            }

            void DrawNodeInterjectEditor(ref EntityAIComponent.PersonalityNode personalityNode)
            {
                GUILayout.Label("On First Loop");
                for (int i = 0; i < personalityNode.onFirstLoopInterjects.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    personalityNode.onFirstLoopInterjects[i] = (InterjectBase) EditorGUILayout.ObjectField("Interject: ", personalityNode.onFirstLoopInterjects[i], typeof(InterjectBase), false);
                    if (GUILayout.Button("Remove"))
                    {
                        personalityNode.onFirstLoopInterjects.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Interject"))
                {
                    personalityNode.onFirstLoopInterjects.Add(new DialogueInterject());
                }
            }
            
            void DrawNodeAbilityEditor(ref EntityAIComponent.PersonalityNode personalityNode)
            {
                GUILayout.Label("Abilities");
                for (int i = 0; i < personalityNode.stateAbilities.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    personalityNode.stateAbilities[i] = (AbilityScriptable) EditorGUILayout.ObjectField("Interject: ", personalityNode.stateAbilities[i], typeof(AbilityScriptable), false);
                    if (GUILayout.Button("Remove"))
                    {
                        personalityNode.stateAbilities.RemoveAt(i);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Add Ability"))
                {
                    personalityNode.stateAbilities.Add(null);
                }
            }

            void DrawNodeMoveSelectEditor(ref EntityAIComponent.PersonalityNode personalityNode)
            {
                MoveSelectCompBase selected = personalityNode.moveSelect;

                EditorGUILayout.Space(12);
                GUILayout.Label("Move Select Personality");

                int currentIndex = 0;
                GUIContent[] msContent = new GUIContent[entityAI.AllMoveSelect.Count];
                for (int i = 0; i < entityAI.AllMoveSelect.Count; i++)
                {
                    msContent[i] = new GUIContent();
                    msContent[i].text = entityAI.AllMoveSelect[i].GetComponentName();
                    if (selected.GetType() == entityAI.AllMoveSelect[i].GetType())
                    {
                        currentIndex = i;
                    }
                }
                int newIndex = EditorGUILayout.Popup(currentIndex, msContent);
                selected = entityAI.AllMoveSelect[newIndex];

                selected.componentName = selected.GetComponentName();

                EditorGUILayout.BeginVertical("HelpBox");
                GUILayout.Label(selected.GetComponentTooltip(), textStyle);
                EditorGUILayout.EndVertical();

                personalityNode.moveSelect = selected;

                if (currentIndex != newIndex)
                {
                    Debug.Log("Should've swapped! selected is now:" + selected.GetComponentName() + ", ai set to: " + personalityNode.moveSelect.GetComponentName());
                }
            }

            void DrawNodeTargetingEditor(ref EntityAIComponent.PersonalityNode personalityNode)
            {
                TargetCompBase selected = personalityNode.targetComp;

                EditorGUILayout.Space(12);
                GUILayout.Label("Targeting Personality");

                int currentIndex = 0;
                GUIContent[] tContent = new GUIContent[entityAI.AllTargeting.Count];
                for (int i = 0; i < entityAI.AllTargeting.Count; i++)
                {
                    tContent[i] = new GUIContent();
                    tContent[i].text = entityAI.AllTargeting[i].GetComponentName();
                    if (selected.GetType() == entityAI.AllTargeting[i].GetType())
                    {
                        currentIndex = i;
                    }
                }
                int newIndex = EditorGUILayout.Popup(currentIndex, tContent);
                selected = entityAI.AllTargeting[newIndex];

                selected.componentName = selected.GetComponentName();

                EditorGUILayout.BeginVertical("HelpBox");
                GUILayout.Label(selected.GetComponentTooltip(), textStyle);
                EditorGUILayout.EndVertical();

                personalityNode.targetComp = selected; 

                if (currentIndex != newIndex)
                {
                    Debug.Log("Should've swapped! selected is now:" + selected.GetComponentName() + ", ai set to: " + personalityNode.targetComp.GetComponentName());
                }
            }
        }
    }
}
