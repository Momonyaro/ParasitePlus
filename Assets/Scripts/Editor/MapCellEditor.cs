using Dungeon;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class MapCellEditor : EditorWindow
    {
        private Vector2 scrollPos = Vector2.zero;
        private Color activeCol = Color.green;
        private Color inactiveCol = Color.red;
        private Color defaultBack = Color.white;

        [MenuItem("Window/Map Cell Editor")]
        public static void ShowWindow()
        {
            GetWindow<MapCellEditor>("Map Cell Editor");
        }

        private void OnGUI()
        {
            defaultBack = GUI.backgroundColor;
            //First filter out the walls, doors and the light.
            List<GameObject> selectedCells = new List<GameObject>();

            foreach (var gObject in Selection.gameObjects)
            {
                if (gObject.GetComponent<MapNode>() != null)
                {
                    selectedCells.Add(gObject);
                }
            }

            if (selectedCells.Count == 0 )
            {
                GUILayout.Label("No Valid Map Node Selected.");
                return;
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();

            DrawMoveControls(selectedCells);

            DrawLightOptions(selectedCells);
            DrawFloorOptions(selectedCells);
            DrawCeilingOptions(selectedCells);

            EditorGUILayout.BeginVertical("HelpBox");
            DrawWallList(selectedCells);
            EditorGUILayout.EndVertical();

            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("HelpBox");
            DrawDoorList(selectedCells);
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();

            GUI.backgroundColor = defaultBack;
        }

        private void DrawMoveControls(List<GameObject> cells)
        {
            int btnSize = 30;
            Vector3 deltaPos = Vector3.zero;
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("NW", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) { deltaPos += new Vector3(-2, 0, 2); }
            if (GUILayout.Button("N", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) { deltaPos += new Vector3(0, 0, 2); }
            if (GUILayout.Button("NE", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) { deltaPos += new Vector3(2, 0, 2); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("W", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) { deltaPos += new Vector3(-2, 0, 0); }
            if (GUILayout.Button("", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) {  }
            if (GUILayout.Button("E", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) { deltaPos += new Vector3(2, 0, 0); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("SW", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) { deltaPos += new Vector3(-2, 0, -2); }
            if (GUILayout.Button("S", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) { deltaPos += new Vector3(0, 0, -2); }
            if (GUILayout.Button("SE", new[] { GUILayout.Width(btnSize), GUILayout.Height(btnSize) })) { deltaPos += new Vector3(2, 0, -2); }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].transform.position += deltaPos;
            }
        }

        private void DrawLightOptions(List<GameObject> cells)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Cell Light Source");
            EditorGUILayout.BeginHorizontal();

            int changeLight = 0;

            if (GUILayout.Button("Enable Lights"))
                changeLight = 1;
            if (GUILayout.Button("Disable Lights"))
                changeLight = -1;

            foreach (var cell in cells)
            {
                List<GameObject> cellChildren = new List<GameObject>();
                for (int i = 0; i < cell.transform.GetChild(0).childCount; i++)
                {
                    cellChildren.Add(cell.transform.GetChild(0).GetChild(i).gameObject);
                }

                for (int i = 0; i < cellChildren.Count; i++)
                {
                    if (cellChildren[i].name.Contains("LAMP_"))
                    {
                        bool lightState = (changeLight == 0) ? cellChildren[i].activeSelf : (changeLight == 1) ? true : false;
                        cellChildren[i].SetActive(lightState);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawFloorOptions(List<GameObject> cells)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Cell Floor");
            EditorGUILayout.BeginHorizontal();

            int changeLight = 0;

            if (GUILayout.Button("Enable Floor"))
                changeLight = 1;
            if (GUILayout.Button("Disable Floor"))
                changeLight = -1;

            foreach (var cell in cells)
            {
                List<GameObject> cellChildren = new List<GameObject>();
                for (int i = 0; i < cell.transform.GetChild(0).childCount; i++)
                {
                    cellChildren.Add(cell.transform.GetChild(0).GetChild(i).gameObject);
                }

                for (int i = 0; i < cellChildren.Count; i++)
                {
                    if (cellChildren[i].name.Contains("FLOOR"))
                    {
                        bool lightState = (changeLight == 0) ? cellChildren[i].activeSelf : (changeLight == 1) ? true : false;
                        cellChildren[i].SetActive(lightState);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawCeilingOptions(List<GameObject> cells)
        {
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("Cell Ceiling");
            EditorGUILayout.BeginHorizontal();

            int changeLight = 0;

            if (GUILayout.Button("Enable Ceiling"))
                changeLight = 1;
            if (GUILayout.Button("Disable Ceiling"))
                changeLight = -1;

            foreach (var cell in cells)
            {
                List<GameObject> cellChildren = new List<GameObject>();
                for (int i = 0; i < cell.transform.GetChild(0).childCount; i++)
                {
                    cellChildren.Add(cell.transform.GetChild(0).GetChild(i).gameObject);
                }

                for (int i = 0; i < cellChildren.Count; i++)
                {
                    if (cellChildren[i].name.Contains("CEILING"))
                    {
                        bool lightState = (changeLight == 0) ? cellChildren[i].activeSelf : (changeLight == 1) ? true : false;
                        cellChildren[i].SetActive(lightState);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void DrawWallList(List<GameObject> cells)
        {
            GameObject exampleCell = cells[0];

            List<GameObject> exChildren = new List<GameObject>();
            for (int i = 0; i < exampleCell.transform.GetChild(0).childCount; i++)
            {
                exChildren.Add(exampleCell.transform.GetChild(0).GetChild(i).gameObject);
            }

            List<GameObject> north = new List<GameObject>();
            List<GameObject> west = new List<GameObject>();
            List<GameObject> east = new List<GameObject>();
            List<GameObject> south = new List<GameObject>();

            List<GameObject> updated = new List<GameObject>();

            foreach (var child in exChildren)
            {
                if (child.name.Contains("WALL_"))
                {
                    if (child.name.Contains("N_"))
                    {
                        north.Add(child);
                    }
                    else if (child.name.Contains("W_"))
                    {
                        west.Add(child);
                    }
                    else if (child.name.Contains("E_"))
                    {
                        east.Add(child);
                    }
                    else if (child.name.Contains("S_"))
                    {
                        south.Add(child);
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("North Walls");
            foreach (var wall in north)
            {
                bool activeState = wall.activeSelf;
                GUI.backgroundColor = (activeState ? activeCol : inactiveCol);
                if (GUILayout.Button(wall.name))
                {
                    wall.SetActive(!activeState);
                    updated.Add(wall);
                }
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("West Walls");
            foreach (var wall in west)
            {
                bool activeState = wall.activeSelf;
                GUI.backgroundColor = (activeState ? activeCol : inactiveCol);
                if (GUILayout.Button(wall.name))
                {
                    wall.SetActive(!activeState);
                    updated.Add(wall);
                }
            }
            EditorGUILayout.EndVertical(); 
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("East Walls");
            foreach (var wall in east)
            {
                bool activeState = wall.activeSelf;
                GUI.backgroundColor = (activeState ? activeCol : inactiveCol);
                if (GUILayout.Button(wall.name))
                {
                    wall.SetActive(!activeState);
                    updated.Add(wall);
                }
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("South Walls");
            foreach (var wall in south)
            {
                bool activeState = wall.activeSelf;
                GUI.backgroundColor = (activeState ? activeCol : inactiveCol);
                if (GUILayout.Button(wall.name))
                {
                    wall.SetActive(!activeState);
                    updated.Add(wall);
                }
            }
            EditorGUILayout.EndVertical(); 
            EditorGUILayout.EndHorizontal();

            // Overwrite other selected cells based on child names.

            foreach (var cell in cells)
            {
                List<GameObject> children = new List<GameObject>();
                for (int i = 0; i < cell.transform.GetChild(0).childCount; i++)
                {
                    children.Add(cell.transform.GetChild(0).GetChild(i).gameObject);
                }

                for (int i = 0; i < children.Count; i++)
                {
                    for (int j = 0; j < updated.Count; j++)
                    {
                        if (children[i].name.Equals(updated[j].name))
                        {
                            children[i].SetActive(updated[j].activeSelf);
                        }
                    }
                }
            }
        }

        private void DrawDoorList(List<GameObject> cells)
        {
            GameObject exampleCell = cells[0];

            List<GameObject> exChildren = new List<GameObject>();
            for (int i = 0; i < exampleCell.transform.GetChild(0).childCount; i++)
            {
                exChildren.Add(exampleCell.transform.GetChild(0).GetChild(i).gameObject);
            }

            List<GameObject> north = new List<GameObject>();
            List<GameObject> west = new List<GameObject>();
            List<GameObject> east = new List<GameObject>();
            List<GameObject> south = new List<GameObject>();

            List<GameObject> updated = new List<GameObject>();

            foreach (var child in exChildren)
            {
                if (child.name.Contains("DOOR_"))
                {
                    if (child.name.Contains("N_"))
                    {
                        north.Add(child);
                    }
                    else if (child.name.Contains("W_"))
                    {
                        west.Add(child);
                    }
                    else if (child.name.Contains("E_"))
                    {
                        east.Add(child);
                    }
                    else if (child.name.Contains("S_"))
                    {
                        south.Add(child);
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("North Doors");
            foreach (var wall in north)
            {
                bool activeState = wall.activeSelf;
                GUI.backgroundColor = (activeState ? activeCol : inactiveCol);
                if (GUILayout.Button(wall.name))
                {
                    wall.SetActive(!activeState);
                    updated.Add(wall);
                }
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("West Doors");
            foreach (var wall in west)
            {
                bool activeState = wall.activeSelf;
                GUI.backgroundColor = (activeState ? activeCol : inactiveCol);
                if (GUILayout.Button(wall.name))
                {
                    wall.SetActive(!activeState);
                    updated.Add(wall);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("East Doors");
            foreach (var wall in east)
            {
                bool activeState = wall.activeSelf;
                GUI.backgroundColor = (activeState ? activeCol : inactiveCol);
                if (GUILayout.Button(wall.name))
                {
                    wall.SetActive(!activeState);
                    updated.Add(wall);
                }
            }
            EditorGUILayout.EndVertical();
            GUI.backgroundColor = defaultBack;
            EditorGUILayout.BeginVertical("Box");
            GUILayout.Label("South Doors");
            foreach (var wall in south)
            {
                bool activeState = wall.activeSelf;
                GUI.backgroundColor = (activeState ? activeCol : inactiveCol);
                if (GUILayout.Button(wall.name))
                {
                    wall.SetActive(!activeState);
                    updated.Add(wall);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            // Overwrite other selected cells based on child names.

            foreach (var cell in cells)
            {
                List<GameObject> children = new List<GameObject>();
                for (int i = 0; i < cell.transform.GetChild(0).childCount; i++)
                {
                    children.Add(cell.transform.GetChild(0).GetChild(i).gameObject);
                }

                for (int i = 0; i < children.Count; i++)
                {
                    for (int j = 0; j < updated.Count; j++)
                    {
                        if (children[i].name.Equals(updated[j].name))
                        {
                            children[i].SetActive(updated[j].activeSelf);
                        }
                    }
                }
            }
        }

    }
}
