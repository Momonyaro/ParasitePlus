using CORE;
using System.Collections;
using System.Collections.Generic;
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
        public AnimationCurve moveFailLerpCurve = new AnimationCurve();

        public UnityEvent onSuccessfulMove;
        private MinimapCompass.Facing lastFacing;
        private Vector3 lastMove = Vector3.zero;

        public HashSet<string> locks = new HashSet<string>();
        private bool turning = false;
        private bool moving = false;

        public bool IsLocked => locks.Count > 0;
        public bool IsMoving => moving;
        public bool IsTurning => turning;
    
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
            walkAction.canceled += OnMoveKey;
            interactAction.started += OnInteractKey;
            pauseAction.started += OnPauseKey;
        }

        private void Start()
        {
            lastFacing = GetNewFacing(transform.rotation.eulerAngles.y);
            FindObjectOfType<MinimapCompass>()?.SetCompassFacing(lastFacing);
        }

        private void OnDisable()
        {
            turnAction.started -= OnTurnKey;
            walkAction.performed -= OnMoveKey;
            walkAction.canceled -= OnMoveKey;
            interactAction.started -= OnInteractKey;
            pauseAction.started -= OnPauseKey;
        }
        
        private void OnDestroy()
        {
            turnAction.started -= OnTurnKey;
            walkAction.performed -= OnMoveKey;
            walkAction.canceled -= OnMoveKey;
            interactAction.started -= OnInteractKey;
            pauseAction.started -= OnPauseKey;
        }

        public void AddLock(string key)
        {
            if (!locks.Contains(key))
                locks.Add(key);
        }

        public void RemoveLock(string key)
        {
            if (locks.Contains(key))
                locks.Remove(key);
        }

        private void Update()
        {

            if (!moving && !turning && !IsLocked)
                StartCoroutine(MovePlayer(lastMove));
        }

        // Check input, we need:
        // Forward, Backward, Strafe left & right
        // and also Turn left & right.

        private void OnMoveKey(InputAction.CallbackContext ctx)
        {
            Vector2 rawVal = ctx.ReadValue<Vector2>();
            lastMove = new Vector2(Mathf.Round(rawVal.x), Mathf.Round(rawVal.y));

        }

        private void OnTurnKey(InputAction.CallbackContext ctx)
        {
            //Read value, start coroutine to turn to the desired rotation based on the value.
            Vector2 rawVal = ctx.ReadValue<Vector2>();
            Vector2 val = new Vector2(Mathf.Round(rawVal.x), Mathf.Round(rawVal.y));

            if (!turning && !moving && !IsLocked)
                StartCoroutine(TurnPlayer(val));
        }

        public void TurnAround()
        {
            if (!turning)
                StartCoroutine(TurnPlayer(new Vector2(2, 0), true));
        }
        
        public void OnInteractKey(InputAction.CallbackContext context)
        {
            DungeonManager dm = FindObjectOfType<DungeonManager>();
            if (!context.started) return;
                if (IsLocked && InfoPrompt.Instance.PromptActive())
                {
                    //This should be the infoPrompt so clear it
                    InfoPrompt.Instance.ClearPrompt();
                }
                else if (!IsLocked)
                {
                    //Check for interactable:s instead
                    dm.CheckToEnterDoorTrigger();
                    dm.CheckToUseInteractable();
                    dm.CheckToPickUpGroundItem();
                }
        }
        
        private void OnPauseKey(InputAction.CallbackContext ctx)
        {
            if (IsLocked && !locks.Contains("PAUSE_MENU")) return;
            
            UIManager.Instance.onUIMessage.Invoke("_togglePauseMenu");
        }

        public void TurnPlayerExt(Vector2 turnDir)
        {
            StartCoroutine(TurnPlayer(turnDir, quiet: true));
        }

        private IEnumerator TurnPlayer(Vector2 turnDir, bool quiet = false)
        {
            turning = true;
            Quaternion rot = transform.rotation;
            Vector3 rotVec = rot.eulerAngles;
            Quaternion nextRot = Quaternion.Euler(0, rotVec.y + (turnDir.x * 90), 0);

            lastFacing = GetNewFacing(nextRot.eulerAngles.y);
            FindObjectOfType<MinimapCompass>().SetCompassFacing(lastFacing);

            float timePassed = 0;
            float maxTime = turnLerpCurve.keys[turnLerpCurve.keys.Length - 1].time;

            if (!quiet)
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_playerTurn", out bool success);

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

        public void MovePlayerExt(Vector2 moveDir)
        {
            StartCoroutine(MovePlayer(moveDir, true));
        }

        private IEnumerator MovePlayer(Vector2 moveDir, bool quiet = false)
        {
            if (moveDir == Vector2.zero) { yield break; }

            moving = true;
            Vector2 initialDir = moveDir;

            float timePassed = 0;
            Vector3 pos = transform.position;
            Vector3 finalPos = pos;
            AnimationCurve lerpCurve = moveLerpCurve;

            RelativeCollisionStruct collisionStruct = CheckMovementCollision();

            //Check collision directions
            if ((collisionStruct.forwardBlocked && moveDir.y > 0) || (collisionStruct.backwardBlocked && moveDir.y < 0))
                moveDir.y = 0;
            if ((collisionStruct.leftBlocked && moveDir.x < 0) || (collisionStruct.rightBlocked && moveDir.x > 0))
                moveDir.x = 0;

            if (moveDir.y != 0)
                finalPos += transform.forward * (moveDir.y * 2.0f);
            else
                finalPos += transform.right * (moveDir.x * 2.0f);



            float maxTime = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

            if (Vector3.Distance(pos, finalPos) < 0.3f && !quiet) 
            {
                lerpCurve = moveFailLerpCurve;

                maxTime = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

                finalPos = pos;
                finalPos += transform.right * initialDir.x;
                finalPos += transform.forward * initialDir.y;

                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_playerStepThud", out bool success);

                while (timePassed < maxTime)
                {
                    transform.position = Vector3.Lerp(pos, finalPos, lerpCurve.Evaluate(timePassed));
                    timePassed += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }

                transform.position = pos;
            }
            else if (Vector3.Distance(pos, finalPos) < 0.3f && quiet)
            {

            }
            else
            {
                //Made a successful move
                SAMSARA.Samsara.Instance.PlaySFXRandomTrack("_playerStepTile", out bool success);
                onSuccessfulMove?.Invoke();

                while (timePassed < maxTime)
                {
                    transform.position = Vector3.Lerp(pos, finalPos, lerpCurve.Evaluate(timePassed));
                    timePassed += Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }

                transform.position = Vector3.Lerp(pos, finalPos, lerpCurve.Evaluate(maxTime));
            }

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

        public void LookAt(MinimapCompass.Facing newFacing)
        {
            float turnDelta = GetTurnDeltaFromFacing(newFacing);
            StartCoroutine(TurnPlayer(new Vector2(turnDelta, 0), quiet: true));
        }

        private MinimapCompass.Facing GetNewFacing(float rotY)
        {
            // change to range 0 - 360

            MinimapCompass.Facing newFacing = MinimapCompass.Facing.NORTH;

            // 0 = N, 90 = E, 180 = S, 270 = W
            if (rotY > 45 && rotY < 135)
                newFacing = MinimapCompass.Facing.EAST;
            else if (rotY > 135 && rotY < 215)
                newFacing = MinimapCompass.Facing.SOUTH;
            else if (rotY > 215 && rotY < 315)
                newFacing = MinimapCompass.Facing.WEST;
            else if (rotY > 315 || rotY < 45)
                newFacing = MinimapCompass.Facing.NORTH;

            return newFacing;
        }

        private float FacingToRot(MinimapCompass.Facing facing)
        {
            // 0 = N, 90 = E, 180 = S, 270 = W

            switch (facing)
            {
                default:
                case MinimapCompass.Facing.NORTH:
                    return 0;
                case MinimapCompass.Facing.EAST:
                    return 90;
                case MinimapCompass.Facing.SOUTH:
                    return 180;
                case MinimapCompass.Facing.WEST:
                    return 270;
            }
        }

        private float GetTurnDeltaFromFacing(MinimapCompass.Facing newFacing)
        {
            switch(lastFacing)
            {
                default:
                case MinimapCompass.Facing.NORTH:
                    switch (newFacing) {
                        default:
                        case MinimapCompass.Facing.NORTH: return 0; 
                        case MinimapCompass.Facing.EAST: return 1;  
                        case MinimapCompass.Facing.SOUTH: return 2;
                        case MinimapCompass.Facing.WEST: return -1;
                    }
                case MinimapCompass.Facing.EAST:
                    switch (newFacing)
                    {
                        default:
                        case MinimapCompass.Facing.EAST: return 0;
                        case MinimapCompass.Facing.SOUTH: return 1;
                        case MinimapCompass.Facing.WEST: return 2;
                        case MinimapCompass.Facing.NORTH: return -1;
                    }
                case MinimapCompass.Facing.SOUTH:
                    switch (newFacing)
                    {
                        default:
                        case MinimapCompass.Facing.SOUTH: return 0;
                        case MinimapCompass.Facing.WEST: return 1;
                        case MinimapCompass.Facing.NORTH: return 2;
                        case MinimapCompass.Facing.EAST: return -1;
                    }
                case MinimapCompass.Facing.WEST:
                    switch (newFacing)
                    {
                        default:
                        case MinimapCompass.Facing.WEST: return 0;
                        case MinimapCompass.Facing.NORTH: return 1;
                        case MinimapCompass.Facing.EAST: return 2;
                        case MinimapCompass.Facing.SOUTH: return -1;
                    }
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
    }
}
