using System;
using CORE;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dialogue.UI
{
    public class CharacterNameBox : MonoBehaviour
    {
        //This is where the player will be able to name the player by using a 2d grid of characters.
        
        //I'm thinking 12 x 5 at the moment for the grid dimensions. If we need more characters, we can add additional grids.
        
        //1,2,3,4,5,6,7,8,9,0,+,-,'
        //A,B,C,D,E,F,G,H,I,J,K,L,M
        //N,O,P,Q,R,S,T,U,V,W,X,Y,Z
        //a,b,c,d,e,f,g,h,i,j,k,l,m
        //n,o,p,q,r,s,t,u,v,w,x,y,z

        private char[] playerNameArray = new char[10]
        {
            'Y', 'o', 'e', 'l', '$', '$', '$', '$', '$', '$' // Use Dollar sign as empty space
        };

        private readonly char[,] alphabetKeyboardGrid = new char[KeyboardHeight, KeyboardWidth]
        {
            {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '+', '-', ' '},
            {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M'},
            {'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'},
            {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm'},
            {'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'},
        };

        private const int KeyboardWidth = 13;
        private const int KeyboardHeight = 5;

        public Vector2Int cursorPos = new Vector2Int(0, 1);
        
        public GameObject inputKeyPrefab = null;
        public Transform playerNameParent = null;
        public Transform keyboardGridParent = null;
        public Transform selectionCursor = null;
        public Transform currentKey = null;

        public SlimComponent.SlimData gameStartSlimData = new SlimComponent.SlimData();

        private void Start()
        {
            ConstructKeyboard();
            ConstructPlayerName();
            UpdateSelectedKey();
        }

        private void Update()
        {
            ShowCurrentNameArrayPos();
            selectionCursor.position = currentKey.position;
        }

        public void MoveCursor(int x, int y)
        {
            cursorPos.x += x;
            cursorPos.y -= y;

            if (cursorPos.x >= KeyboardWidth)
                cursorPos.x -= KeyboardWidth;
            if (cursorPos.x <  0)
                cursorPos.x += KeyboardWidth;
            
            if (cursorPos.y >= KeyboardHeight)
                cursorPos.y -= KeyboardHeight;
            if (cursorPos.y <  0)
                cursorPos.y += KeyboardHeight;
            
            UpdateSelectedKey();
        }

        public void AddSelectedToPlayerName()
        {
            char selected = alphabetKeyboardGrid[cursorPos.y, cursorPos.x];
            
            for (int i = 0; i < playerNameArray.Length; i++)
            {
                if (playerNameArray[i] == '$')
                {
                    playerNameArray[i] = selected;
                    ConstructPlayerName();
                    return;
                }
            }
        }

        public void RemoveLastFromPlayerName()
        {
            for (int i = 0; i < playerNameArray.Length; i++)
            {
                if (playerNameArray[i] == '$')
                {
                    int prevIndex = Mathf.Clamp(i - 1, 0, playerNameArray.Length - 1);
                    playerNameArray[prevIndex] = '$';
                    ConstructPlayerName();
                    return;
                }
            }
            
            playerNameArray[playerNameArray.Length - 1] = '$';
            ConstructPlayerName();
        }
        
        private void UpdateSelectedKey()
        {
            //Get the gameobject at the current cursorPos
            int currentKeyIndex = cursorPos.x + (cursorPos.y * (KeyboardWidth));

            currentKey = keyboardGridParent.GetChild(currentKeyIndex);
        }

        public void VerifyNameAndCreateSlim()
        {
            string name = VerifyPlayerName();

            for (int i = 0; i < gameStartSlimData.partyField.Length; i++)
            {
                if (gameStartSlimData.partyField[i] == null) continue;

                gameStartSlimData.partyField[i] = gameStartSlimData.partyField[i].Copy();
            }

            gameStartSlimData.playerName = name;
            gameStartSlimData.partyField[0].entityName = name;
            
            CreateGameStartSlim();
        }

        public void CreateGameStartSlim()
        {
            SlimComponent.Instance.PopulateAndSendSlim(gameStartSlimData);
        }

        private void ShowCurrentNameArrayPos()
        {
            for (int i = 0; i < playerNameArray.Length; i++)
            {
                if (playerNameArray[i] == '$')
                {
                    playerNameParent.GetChild(i).GetComponent<Selectable>().OnSelect(null);
                    return;
                }
            }
        }
        
        private void ConstructKeyboard()
        {
            int x = 0;
            int y = 0;

            for (int i = 0; i < alphabetKeyboardGrid.Length; i++)
            {

                GameObject instance = Instantiate(inputKeyPrefab, keyboardGridParent);

                Transform instanceTransform = instance.transform;
                instanceTransform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{alphabetKeyboardGrid[y, x]}";
                instance.name = $"{alphabetKeyboardGrid[y, x]}";
                
                x++;
                if (x >= 13)
                {
                    x = 0;
                    y++;
                }
            }
        }

        private void ConstructPlayerName()
        {
            for (int i = 0; i < playerNameParent.childCount; i++)
            {
                Destroy(playerNameParent.GetChild(i).gameObject);
            }

            for (int i = 0; i < playerNameArray.Length; i++)
            {
                GameObject instance = Instantiate(inputKeyPrefab, playerNameParent);

                Transform instanceTransform = instance.transform;

                char toWrite = (playerNameArray[i] == '$') ? ' ' : playerNameArray[i];
                instanceTransform.GetChild(0).GetComponent<TextMeshProUGUI>().text = $"{toWrite}";
            }
        }

        private string VerifyPlayerName()
        {
            int x = 0;
            int y = 0;

            string result = "";
            
            for (int q = 0; q < playerNameArray.Length; q++)
            {
                for (int p = 0; p < alphabetKeyboardGrid.Length; p++)
                {
                    if (alphabetKeyboardGrid[y, x] == playerNameArray[q])
                    {
                        result += playerNameArray[q];
                        break;
                    }
                    
                    x++;
                    if (x >= 13)
                    {
                        x = 0;
                        y++;
                    }
                }

                x = 0;
                y = 0;
            }

            return result;
        }

    }
}