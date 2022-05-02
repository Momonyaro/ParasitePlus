using System;
using System.Collections;
using CORE;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace MOVEMENT
{
    public class FPSGridPlayer : MonoBehaviour
    {
        public InputActionAsset playerActions;
        private InputActionMap actionMap;
        private InputAction walkAction;
        private InputAction turnAction;
        private InputAction interactAction;
        private InputAction pauseAction;

        public AnimationCurve turnLerpCurve = new AnimationCurve();
        public AnimationCurve moveLerpCurve = new AnimationCurve();
        
        public UnityEvent onSuccessfulMove;

        public bool lockPlayer = false;
        private bool turning = false;
        private bool moving = false;
    
        private void Awake()
        {
            ReAlignWithGrid();
            actionMap = playerActions.FindActionMap("Player");
            walkAction = actionMap.FindAction("Move");
            turnAction = actionMap.FindAction("Look");
            interactAction = actionMap.FindAction("Interact");
            pauseAction = actionMap.FindAction("StatusMenu");

            turnAction.started += OnTurnKey;
            walkAction.performed += OnMoveKey;
            interactAction.started += OnInteractKey;
            pauseAction.started += OnPauseKey;
        }

        private void OnDisable()
        {
            turnAction.started -= OnTurnKey;
            walkAction.performed -= OnMoveKey;
            interactAction.started -= OnInteractKey;
        }
        
        private void OnDestroy()
        {
            turnAction.started -= OnTurnKey;
            walkAction.performed -= OnMoveKey;
            interactAction.started -= OnInteractKey;
        }

        // Check input, we need:
        // Forward, Backward, Strafe left & right
        // and also Turn left & right.

        private void OnMoveKey(InputAction.CallbackContext ctx)
        {
            Vector2 rawVal = ctx.ReadValue<Vector2>();
            Vector2 val = new Vector2(Mathf.Round(rawVal.x), Mathf.Round(rawVal.y));

            if (!moving && !turning && !lockPlayer)
                StartCoroutine(MovePlayer(val));
        }

        private void OnTurnKey(InputAction.CallbackContext ctx)
        {
            //Read value, start coroutine to turn to the desired rotation based on the value.
            Vector2 rawVal = ctx.ReadValue<Vector2>();
            Vector2 val = new Vector2(Mathf.Round(rawVal.x), Mathf.Round(rawVal.y));

            if (!turning && !moving && !lockPlayer)
                StartCoroutine(TurnPlayer(val));
        }
        
        public void OnInteractKey(InputAction.CallbackContext context)
        {
            DungeonManager dm = FindObjectOfType<DungeonManager>();
            if (!context.started) return;
                if (lockPlayer && InfoPrompt.Instance.PromptActive())
                {
                    //This should be the infoPrompt so clear it
                    InfoPrompt.Instance.ClearPrompt();
                }
                else if (!lockPlayer)
                {
                    //Check for interactable:s instead
                    dm.CheckToEnterDoorTrigger();
                    dm.CheckToUseInteractable();
                    dm.CheckToPickUpGroundItem();
                }
        }
        
        private void OnPauseKey(InputAction.CallbackContext ctx)
        {
            if (lockPlayer) return;

            lockPlayer = true;
            
            UIManager.Instance.onUIMessage.Invoke("_toggleStatusMenu");
        }

        private IEnumerator TurnPlayer(Vector2 turnDir)
        {
            turning = true;
            Quaternion rot = transform.rotation;
            Vector3 rotVec = rot.eulerAngles;
            Quaternion nextRot = Quaternion.Euler(0, rotVec.y + (turnDir.x * 90), 0);

            float timePassed = 0;
            float maxTime = turnLerpCurve.keys[turnLerpCurve.keys.Length - 1].time;

            while (timePassed < maxTime)
            {
                transform.rotation = Quaternion.Lerp(rot, nextRot, turnLerpCurve.Evaluate(timePassed));
                
                timePassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            transform.rotation = Quaternion.Lerp(rot, nextRot, turnLerpCurve.Evaluate(maxTime));
            turning = false;
            
            yield break;
        }

        private IEnumerator MovePlayer(Vector2 moveDir)
        {
            // X-axis is strafe, Y-Axis is forwards and back
            moving = true;
            float timePassed = 0;
            float maxTime = moveLerpCurve.keys[moveLerpCurve.keys.Length - 1].time;
            Vector3 pos = transform.position;
            Vector3 finalPos = pos;
            RelativeCollisionStruct collisionStruct = CheckMovementCollision();

            if ((collisionStruct.forwardBlocked && moveDir.y > 0) || (collisionStruct.backwardBlocked && moveDir.y < 0))
                moveDir.y = 0;
            if ((collisionStruct.leftBlocked && moveDir.x < 0) || (collisionStruct.rightBlocked && moveDir.x > 0))
                moveDir.x = 0;

            if (moveDir.y != 0)
                finalPos += transform.forward * (moveDir.y * 2.0f);
            else
                finalPos += transform.right * (moveDir.x * 2.0f);

            if (Vector3.Distance(pos, finalPos) > 0.8f)
            {
                //We actually moved, let's fire the event to increase the encounterProgress.
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_playerStepTile", out bool success);
                onSuccessfulMove?.Invoke();
            }

            while (timePassed < maxTime)
            {
                transform.position = Vector3.Lerp(pos, finalPos, moveLerpCurve.Evaluate(timePassed));
                timePassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            
            transform.position = Vector3.Lerp(pos, finalPos, moveLerpCurve.Evaluate(maxTime));

            moving = false;
            
            yield break;
        }

        private void ReAlignWithGrid()
        {
            Vector3 pos = transform.position;

            pos.x = Mathf.Round(pos.x);
            pos.y = Mathf.Round(pos.y);
            pos.z = Mathf.Round(pos.z);

            transform.position = pos;
        }

        private RelativeCollisionStruct CheckMovementCollision()
        {
            Vector3 pos = transform.position;
            Vector3 sample = transform.forward;
            
            RaycastHit hit;
            RelativeCollisionStruct result = new RelativeCollisionStruct();
            for (int i = 0; i < 4; i++) // Forward, right, backward, left
            {
                Physics.Raycast(pos, sample, out hit, 1.5f);

                bool wasHit = (hit.collider != null && !hit.collider.isTrigger);

                switch (i)
                {
                    case 0: // forward
                        result.forwardBlocked = wasHit;
                        break;
                    case 1: // right
                        result.rightBlocked = wasHit;
                        break;
                    case 2: // backward
                        result.backwardBlocked = wasHit;
                        break;
                    default: // left
                        result.leftBlocked = wasHit;
                        break;
                }

                sample = Quaternion.Euler(0, 90, 0) * sample;
            }

            return result;
        }

        private void OnDrawGizmos()
        {
            RelativeCollisionStruct collisionResult = CheckMovementCollision();
            
            Vector3 pos = transform.position;
            Vector3 sample = transform.forward;
            
            Gizmos.DrawIcon(pos, "SoftlockProjectBrowser Icon");
            
            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0: // forward
                        Gizmos.color = (collisionResult.forwardBlocked) ? Color.red : Color.green;
                        Gizmos.DrawLine(pos, pos + sample);
                        Gizmos.DrawLine(pos + sample, Quaternion.Euler(0, 30, 0) * sample * 0.5f + pos);
                        Gizmos.DrawLine(pos + sample, Quaternion.Euler(0, -30, 0) * sample * 0.5f + pos);
                        break;
                    case 1: // right
                        Gizmos.color = (collisionResult.rightBlocked) ? Color.red : Color.green;
                        Gizmos.DrawLine(pos, pos + sample);
                        break;
                    case 2: // backward
                        Gizmos.color = (collisionResult.backwardBlocked) ? Color.red : Color.green;
                        Gizmos.DrawLine(pos, pos + sample);
                        break;
                    default: // left
                        Gizmos.color = (collisionResult.leftBlocked) ? Color.red : Color.green;
                        Gizmos.DrawLine(pos, pos + sample);
                        break;
                }
                
                sample = Quaternion.Euler(0, 90, 0) * sample;
            }
        }

        public struct RelativeCollisionStruct
        {
            public bool forwardBlocked;
            public bool backwardBlocked;
            public bool leftBlocked;
            public bool rightBlocked;

            public RelativeCollisionStruct(bool fuck = false)
            {
                forwardBlocked = false;
                backwardBlocked = false;
                leftBlocked = false;
                rightBlocked = false;
            }
        }
    }
}
