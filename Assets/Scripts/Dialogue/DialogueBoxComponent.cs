using System.Collections.Generic;
using CORE;
using Dialogue.UI;
using SAMSARA;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Dialogue
{
    [System.Serializable]
    public class DialogueBoxComponent : DialogueComponent
    {
        public List<DialogueBox> dialogueBoxes = new List<DialogueBox>();
        public bool perSentenceWriteToScreen = true;
        public bool requireContinueInput = true;
        public string writingSoundEvent = "";
        private int currentIndex = 0;
        private int currentChar = 0;
        private float buildTimer = 0;
        private bool canBuild = false;
        private bool skipBuild = false;
        private bool buildingText = false;
        private string playerName = "Yoel";

        private const string PlayerNameMacro = "$Player";

        private DialogueScriptable dialogueScriptable;
        
        public override void Init(DialogueScriptable parent, out GameObject componentPrefab)
        {
            errorAttempts = 99;
            currentIndex = 0;
            canBuild = true;
            SetNewInstance(null); // Nullify instance so that we don't have any ghosts spooking the code.

            dialogueScriptable = parent;

            playerName = SlimComponent.Instance.ReadNonVolatilePlayerName();
            
            componentPrefab = objectPrefab;
        }

        public override void Update(out bool endOfLife)
        {
            //Check if the instance is null before doing anything

            if (AtEndOfLife()) { endOfLife = true; return; }
            
            SlowWriteToReciever();

            if (!requireContinueInput && !buildingText)
            {
                currentIndex++;
                currentIndex = Mathf.Clamp(currentIndex, 0, dialogueBoxes.Count);
                canBuild = true;
            }
            
            endOfLife = false;
        }

        public override void OnSubmitInput(InputAction.CallbackContext context)
        {
            // Used for skipping the string building and for advancing to the next text block until endOfLife
            
            if (buildingText) // This means to skip the building and just write it out.
            {
                skipBuild = true;
            }
            else // Advance the currentIndex and flag buildable
            {
                currentIndex++;
                currentIndex = Mathf.Clamp(currentIndex, 0, dialogueBoxes.Count);
                canBuild = true;
                
                Debug.Log("Dialogue Box Component with ref: " + reference + " updated currentIndex to: " + currentIndex);
                
                if (IsNull()) return;
                DialogueBoxReciever reciever = GetCurrentInstance().GetComponent<DialogueBoxReciever>();
                if (reciever.continuePrompt != null)
                    reciever.continuePrompt.SetActive(false);
            }
        }

        public override void OnCancelInput(InputAction.CallbackContext context)
        {
            // We're not using this
        }

        public override void OnMoveInput(InputAction.CallbackContext context)
        {
            // We're not using this
        }

        private void SlowWriteToReciever()
        {
            if (IsNull()) return;
            if (dialogueBoxes[currentIndex].buildTime == 0)
                skipBuild = true;
            
            DialogueBoxReciever reciever = GetCurrentInstance().GetComponent<DialogueBoxReciever>();
            DialogueBox current = dialogueBoxes[currentIndex];
            string currentText = current.text;

            currentText = currentText.Replace(PlayerNameMacro, playerName);
            
            Speaker currentSpeaker = GetCurrentSpeaker();
            if (reciever.speakerPhoto != null)
                reciever.speakerPhoto.sprite = currentSpeaker.speakerPhoto;
            if (reciever.speakerName != null)
                reciever.speakerName.text = currentSpeaker.speakerName;
            
            // How do make string builder? Me 2 dumb?
            if (skipBuild)
            {
                reciever.dialogueText.text = currentText;
                skipBuild = false;
                buildTimer = current.buildTime;
                currentChar = 0;
                buildingText = false;
                canBuild = false;
            }
            
            if (canBuild && !buildingText) // Set settings before going to work
            {
                reciever.dialogueText.text = "";
                buildTimer = current.buildTime;
                currentChar = 0;
                buildingText = true;
            }
            else if (buildingText)
            {
                buildTimer -= Time.deltaTime;
                if (buildTimer <= 0.0f)
                {
                    bool playSound = true;
                    if (perSentenceWriteToScreen)
                    {
                        string nextSection = "";
                        buildTimer = 0;
                        while (true)
                        {
                            char nextChar = currentText.ToCharArray()[currentChar];

                            nextSection += nextChar;
                            buildTimer += current.buildTime;
                            currentChar++;

                            if (nextChar == '.' || nextChar == ' ')
                            {
                                buildTimer += current.buildTime;
                                playSound = false;
                                break;
                            }

                            if (currentChar >= currentText.Length)
                                break;
                        }

                        reciever.dialogueText.text += nextSection;
                    }
                    else
                    {
                        //Place character
                        char next = currentText.ToCharArray()[currentChar];
                        reciever.dialogueText.text += next;
                        currentChar++;

                        if (next == '<')
                        {
                            while (true)
                            {
                                next = currentText.ToCharArray()[currentChar];
                                reciever.dialogueText.text += next;
                                currentChar++;
                                if (next == '>') break;
                            }
                        }

                        buildTimer = current.buildTime;
                        
                        if (next == '.' || next == ' ' || next == ',')
                        {
                            playSound = false;
                            buildTimer = current.buildTime * 1.5f;
                        }
                    }
                    
                    if (playSound)
                        Samsara.Instance.PlaySFXRandomTrack(writingSoundEvent, out bool success);

                    if (currentChar >= currentText.Length)
                    {
                        buildingText = false;
                        canBuild = false;
                    }
                }
            }
            else
            {
                if (reciever.continuePrompt != null)
                    reciever.continuePrompt.SetActive(true);
            }
        }

        public override ComponentTypes GetComponentType()
        {
            return ComponentTypes.DIALOGUE_BOX;
        }

        public override bool PlaceInBackground()
        {
            return false;
        }

        private bool AtEndOfLife()
        {
            return (currentIndex >= dialogueBoxes.Count);
        }

        public Speaker GetCurrentSpeaker()
        {
            Speaker speaker = dialogueScriptable.GetSpeakerFromReference(dialogueBoxes[currentIndex].speakerReference, out bool success);
            if (!success) Debug.LogError($"Failed to fetch speaker using reference: {dialogueBoxes[currentIndex].speakerReference}, expect nonsense data");
            return speaker;
        }

        [System.Serializable]
        public struct DialogueBox
        {
            public string speakerReference;
            public string text;
            public float buildTime;

            public DialogueBox(Speaker speaker, string text)
            {
                this.speakerReference = speaker.speakerReference;
                this.text = text;
                this.buildTime = 0.02f;
            }
            
            public DialogueBox(string speakerReference, string text)
            {
                this.speakerReference = speakerReference;
                this.text = text;
                this.buildTime = 0.02f;
            }
        }
    }
}
