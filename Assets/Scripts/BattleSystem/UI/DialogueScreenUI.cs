using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleSystem.UI
{
    public class DialogueScreenUI : MonoBehaviour
    {
        public GameObject parent;
        public Image background;
        public Image speakerPortrait;
        public TextMeshProUGUI text;
        public float buildDelay;

        private Queue<DialogueNode> currentDialogue = new Queue<DialogueNode>();
        [SerializeField] private List<DialogueNode> debugInfo = new List<DialogueNode>();
        private bool running = false;
        private bool writing = false;
        private bool skipWriting = false;
        
        public void StartDialogue(List<DialogueNode> newDialogue)
        {
            running = true;

            Time.timeScale = 0f;
            
            currentDialogue = new Queue<DialogueNode>(newDialogue);
            debugInfo = new List<DialogueNode>(newDialogue);
            parent.SetActive(true);
            StartCoroutine(WriteNextNode());
        }

        public bool UpdateDialogue()
        {
            if (currentDialogue.Count == 0 && !writing)
            {
                parent.SetActive(false);
                Time.timeScale = 1.0f;
                running = false;
                return false;
            }

            if (writing)
            {
                skipWriting = true;
            }
            else
            {
                StartCoroutine(WriteNextNode());
            }
            
            return true;
        }

        public bool IsRunning()
        {
            return running;
        }

        private IEnumerator WriteNextNode()
        {
            if (currentDialogue.Count == 0)
            {
                yield break;
            }
            
            writing = true;
            
            DialogueNode node = currentDialogue.Dequeue();

            background.color = node.backgroundColor;
            speakerPortrait.sprite = node.speakerPortrait;
            buildDelay = node.buildDelay;

            string prefix = "";

            string content = node.text;
            if (node.text.Contains(":"))
            {
                int nodePos = node.text.IndexOf(":", StringComparison.Ordinal);
                prefix = node.text.Substring(0, nodePos + 1);
                content = content.Replace(prefix, "");
            }
            text.text = $"<color=yellow>{prefix}</color>";

            Queue<char> textToWrite = new Queue<char>(content);

            while (textToWrite.Count > 0)
            {
                yield return new WaitForSecondsRealtime(buildDelay);

                bool foundFormatting = false;
                string nextMsg = "";
                while (true)
                {
                    char next = textToWrite.Dequeue();
                    nextMsg += next;
                    if (next == '<')
                    {
                        foundFormatting = true;
                    }
                    
                    if (next == '>')
                    {
                        foundFormatting = false;
                    }

                    if (foundFormatting)
                    {
                        continue;
                    }

                    break;
                }
                text.text += nextMsg;

                if (skipWriting)
                {
                    text.text = $"<color=yellow>{prefix}</color>{content}";
                    writing = false;
                    skipWriting = false;
                    yield break;
                }
            }
            
            text.text = $"<color=yellow>{prefix}</color>{content}";
            writing = false;
        }
    }
}
