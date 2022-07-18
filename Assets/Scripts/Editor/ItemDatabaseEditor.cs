using System;
using System.Collections.Generic;
using CORE;
using Items;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ItemDatabase))]
    public class ItemDatabaseEditor : UnityEditor.Editor
    {
        private Item selected = null;
        
        public override void OnInspectorGUI()
        {
            var database = (ItemDatabase) target;
            
            //base.OnInspectorGUI();
            
            //Idea: Top has an "Add Item button", below is a window for detailing a selected item where you can edit the details, below that is a button to delete that item if you want
            // and finally there is a list of all the items, sorted after type.

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Item", GUILayout.Height(40)))
            {
                ItemType newItemType = (selected == null) ? ItemType.MISC : selected.type;
                
                database.itemDatabase.Add(new Item("New DB Entry", newItemType, 0, 0, 0, 0, 0, 0, 0));
                selected = database.itemDatabase[database.itemDatabase.Count - 1];
                
                EditorUtility.SetDirty(target);
            }
            Color fallback = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.14f, 0.93f, 0.15f);
            if (GUILayout.Button("Mark Dirty", GUILayout.Height(40), GUILayout.Width(80)))
            {
                EditorUtility.SetDirty(target);
            }
            GUI.backgroundColor = fallback;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
            
            DrawItemPreview();
            
            EditorGUILayout.Space(10);
            
            SortAndDrawItemIndex(database);
        }

        private void DrawItemPreview()
        {
            //Draw selected if not null
            if (selected == null) return;

            GUILayout.Label("Selected Item Preview");
            EditorGUILayout.BeginVertical("HelpBox");
            
            selected.name = EditorGUILayout.TextField("Item Name:", selected.name);
            EditorGUILayout.BeginHorizontal();
            GUILayout.TextField(selected.guid);
            if (GUILayout.Button("Generate new GUID"))
            {
                selected.guid = Guid.NewGuid().ToString();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            selected.msrp = EditorGUILayout.IntField("Item MSRP:", selected.msrp);
            selected.type = (ItemType) EditorGUILayout.EnumPopup(selected.type);
            EditorGUILayout.EndHorizontal();
            
            EditorGUIUtility.labelWidth = 120;
            EditorGUILayout.BeginHorizontal();
            selected.minLevelReq = EditorGUILayout.IntField("Min Level Req:", selected.minLevelReq);
            selected.minFriendshipReq = EditorGUILayout.IntField("Min Friendship Req:", selected.minFriendshipReq);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            
            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            selected.damage = EditorGUILayout.IntField("Damage:", selected.damage);
            selected.defense = EditorGUILayout.IntField("Defense:", selected.defense);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            selected.critChance = EditorGUILayout.FloatField("Crit Chance:", selected.critChance);
            selected.dodgeChance = EditorGUILayout.FloatField("Dodge Chance:", selected.dodgeChance);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginHorizontal();
            selected.stackable = EditorGUILayout.Toggle("Stackable: ", selected.stackable);
            EditorGUI.BeginDisabledGroup(!selected.stackable);
            selected.StackSize = EditorGUILayout.Vector2IntField("Stack:", selected.StackSize);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            selected.itemAbility = (Scriptables.AbilityScriptable)EditorGUILayout.ObjectField("Item Ability: ", selected.itemAbility, typeof(Scriptables.AbilityScriptable), false);

            EditorGUIUtility.labelWidth = 250;
            
            GUILayout.Space(20);

            if (GUILayout.Button("Remove Selected Item"))
            {
                RemoveItemFromDBUsingGuid(selected.guid);
                EditorUtility.SetDirty(target);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void SortAndDrawItemIndex(ItemDatabase database)
        {
            //Let's do this in a stupid way ey?
            List<Item>[] sortingBuckets = new List<Item>[7];

            for (int q = 0; q < sortingBuckets.Length; q++)
            {
                sortingBuckets[q] = new List<Item>();
            }

            for (int i = 0; i < database.itemDatabase.Count; i++)
            {
                switch (database.itemDatabase[i].type)
                {
                    case ItemType.CLUB:
                        sortingBuckets[0].Add(database.itemDatabase[i]);
                        break;
                    case ItemType.GUN:
                        sortingBuckets[1].Add(database.itemDatabase[i]);
                        break;
                    case ItemType.KNIFE:
                        sortingBuckets[2].Add(database.itemDatabase[i]);
                        break;
                    case ItemType.AID:
                        sortingBuckets[3].Add(database.itemDatabase[i]);
                        break;
                    case ItemType.ARMOR:
                        sortingBuckets[4].Add(database.itemDatabase[i]);
                        break;
                    case ItemType.KEY:
                        sortingBuckets[5].Add(database.itemDatabase[i]);
                        break;
                    default:
                        sortingBuckets[6].Add(database.itemDatabase[i]);
                        break;
                }
            }
            
            for (int q = 0; q < sortingBuckets.Length; q++)
            {
                string label = "";
                switch (q)
                {
                    case 0: label = "Clubs"; break;
                    case 1: label = "Guns"; break;
                    case 2: label = "Knives"; break;
                    case 3: label = "Aid"; break;
                    case 4: label = "Armor"; break;
                    case 5: label = "Keys"; break;
                    case 6: label = "Misc."; break;
                }
                GUILayout.Label(label);
                
                EditorGUILayout.BeginVertical("HelpBox");
                
                Color fallback = GUI.backgroundColor;
                for (int i = 0; i < sortingBuckets[q].Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUIUtility.labelWidth = 120;
                    sortingBuckets[q][i].storeItem = EditorGUILayout.Toggle(sortingBuckets[q][i].storeItem, GUILayout.Width(20));
                    EditorGUIUtility.labelWidth = 250;
                    
                    if (GUILayout.Button(sortingBuckets[q][i].name, GUILayout.Height(20), GUILayout.MaxWidth(800)))
                    {
                        selected = GetItemFromDBUsingGuid(sortingBuckets[q][i].guid);
                    }

                    GUI.backgroundColor = new Color(0.93f, 0.31f, 0.26f);
                    if (GUILayout.Button("x", GUILayout.Height(20), GUILayout.Width(20)))
                    {
                        RemoveItemFromDBUsingGuid(sortingBuckets[q][i].guid);
                        return;
                    }
                    GUI.backgroundColor = fallback;
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
        }

        private Item GetItemFromDBUsingGuid(string guid)
        {
            var database = (ItemDatabase) target;

            for (int i = 0; i < database.itemDatabase.Count; i++)
            {
                if (database.itemDatabase[i].guid.Equals(guid))
                    return database.itemDatabase[i];
            }

            return null;
        }
        
        private void RemoveItemFromDBUsingGuid(string guid)
        {
            var database = (ItemDatabase) target;

            for (int i = 0; i < database.itemDatabase.Count; i++)
            {
                if (database.itemDatabase[i].guid.Equals(guid))
                {
                    database.itemDatabase.RemoveAt(i);
                    selected = null;
                    return;
                }
            }
        }
    }
}
