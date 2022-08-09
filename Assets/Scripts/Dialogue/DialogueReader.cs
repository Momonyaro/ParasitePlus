using System;
using System.Collections.Generic;
using MOVEMENT;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Dialogue
{
    public class DialogueReader : MonoBehaviour
    {
        [Header("Dialogue Data")]
        public DialogueScriptable dialogueData;
        public bool initOnStart = false;
        public UnityEvent onStopReading = new UnityEvent();
        
        [Space(15)]
        
        public GameObject backgroundFather;
        public GameObject allFather;
        
        private Queue<DialogueComponent> executionQueue = new Queue<DialogueComponent>();
        private bool dialogueRunning = false;
        private DialogueComponent lastComponent = null;
        
        [SerializeField] private InputActionAsset module;
        [SerializeField] private FPSGridPlayer player;
        private InputActionMap inputActionMap;
        private InputAction submit;
        private InputAction cancel;
        private InputAction move;
        private InputAction start;

        public bool IsRunning => dialogueRunning;

        private void Awake()
        {
            inputActionMap = module.FindActionMap("UI");
            submit = inputActionMap.FindAction("Submit");
            cancel = inputActionMap.FindAction("Cancel");
            move = inputActionMap.FindAction("Move");
            start = inputActionMap.FindAction("Start");
            
            submit.started += PassInputSubmit;
            cancel.started += PassInputCancel;
            move.started += PassInputMove;
            start.started += PassInputStart;
        }
        
        // Start is called before the first frame update
        void Start()
        {
            if (initOnStart)
                StartDialogue();
        }

        private void Update()
        {
            if (!dialogueRunning) return;

            if (player != null)
                player.lockPlayer = true;

            lastComponent.Update(out bool endOfLife);
            
            if (endOfLife) CycleComponent();
        }

        public void StartDialogue()
        {
            PopulateExecQueue();

            lastComponent = executionQueue.Dequeue();
            Debug.Log($"Dialogue: Started Read of [" + dialogueData.name + "]");
            InitComponent(ref lastComponent);
            dialogueRunning = true;

            if (player != null)
                player.lockPlayer = true;
        }

        private void CycleComponent()
        {
            if (executionQueue.Count == 0)
            {
                EndDialogue();
                return;
            }

            while (true)
            {
                if (executionQueue.Count == 0)
                {
                    EndDialogue();
                    break;
                }
                
                lastComponent = executionQueue.Dequeue();
                InitComponent(ref lastComponent);
                lastComponent.Update(out bool endOfLife);

                if (!endOfLife)
                    break;
            }
        }

        private void PassInputSubmit(InputAction.CallbackContext context)
        {
            lastComponent?.OnSubmitInput(context);
        }

        private void PassInputCancel(InputAction.CallbackContext context)
        {
            lastComponent?.OnCancelInput(context);
        }

        private void PassInputMove(InputAction.CallbackContext context)
        {
            lastComponent?.OnMoveInput(context);
        }
        
        private void PassInputStart(InputAction.CallbackContext context)
        {
            lastComponent?.OnStartInput(context);
        }

        private void EndDialogue()
        {
            dialogueRunning = false;
            lastComponent = null;

            if (dialogueData.transitionToOnEof != null)
            {
                dialogueData = dialogueData.transitionToOnEof;
                
                WipeOldData();
                StartDialogue();
                return;
            }

            if (dialogueData.goToSceneOnEof)
            {
                SceneParser.ParseSceneChange(dialogueData.destinationSceneOnEof, out string slimDestination, out string destination);

                CORE.SlimComponent.Instance.SetNonVolatileDestination(slimDestination);
                SceneManager.LoadScene(destination);
            }

            onStopReading?.Invoke();
            
            if (player != null)
                player.lockPlayer = false;
            
            Debug.Log($"Dialogue: End of Read");
        }

        private void InitComponent(ref DialogueComponent component)
        {
            Debug.Log($"Initializing Component of type: {component.GetComponentType()}, with ref: {component.reference}");

            component.Init(dialogueData, out var toInstantiate);

            if (toInstantiate != null)
            {
                Transform father = allFather.transform;
                if (component.PlaceInBackground())
                    father = backgroundFather.transform;
                
                GameObject instance = Instantiate(toInstantiate, father);
                RectTransform rectTransform = instance.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = component.screenPosition;
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                component.SetNewInstance(instance);
            }
        }

        private void WipeOldData()
        {
            for (int i = 0; i < allFather.transform.childCount; i++)
            {
                Destroy(allFather.transform.GetChild(i).gameObject);
            }
            for (int i = 0; i < backgroundFather.transform.childCount; i++)
            {
                Destroy(backgroundFather.transform.GetChild(i).gameObject);
            }
        }

        private void PopulateExecQueue()
        {
            List<DialogueComponent> dialogueComponents = dialogueData.components;
            executionQueue.Clear();
            for (int i = 0; i < dialogueComponents.Count; i++)
            {
                executionQueue.Enqueue(dialogueComponents[i]);
            }
        }
    }
}
