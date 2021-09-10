using System;
using System.Collections;
using CORE;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MOVEMENT
{
    public enum Facing
    {
        AWAY,
        TOWARDS,
        LEFT,
        RIGHT
    }
    
    public class PlayerMovement : MonoBehaviour
    {
        public Transform camPivot;
        public Transform playerGraphic;
        public SpriteRenderer spriteRenderer;
        public Transform mapPin;
        public Transform mapTex;
        [Range(0.1f, 14f)] public float playerSpeed = 1;
        [Range(0, 1)] public float stickDeadzone = 0.05f;
        [Range(0.1f, 10f)] public float cameraSpeed = 1;
        public float mouseSensetivity = 0.1f;
        public AnimationScriptable idleAnim;
        public AnimationScriptable  runAnim;
        private bool snappingRot = false;
        private Facing playerFacing = Facing.AWAY;
        public bool lockPlayer = false;
        public bool shopping = false;

        private Vector2 camRotDelta = Vector2.zero;
        private Vector3 mvmtDelta = Vector3.zero;
        private Rigidbody rigidbody;

        private void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            // if (!snappingRot && Input.GetKeyDown(KeyCode.Tab)) StartCoroutine(SnapToBack());
            if (lockPlayer)
            {
                mvmtDelta = Vector3.zero;
                camRotDelta = Vector2.zero;
                return;
            }

            var camRotDir = camRotDelta;

            float cameraRotVal = 0;
            if (snappingRot || (camRotDir.x > stickDeadzone && camRotDir.y < -stickDeadzone)) {}
            else if (camRotDir.x >  stickDeadzone) //Right
            {
                cameraRotVal = camRotDir.x * cameraSpeed;
            }
            else if (camRotDir.y < -stickDeadzone) //Left
            {
                cameraRotVal = camRotDir.y * cameraSpeed;
            }
            if (!snappingRot) camPivot.Rotate(Vector3.up, cameraRotVal * Time.deltaTime * 10);

            AnimationScriptable currentAnim = idleAnim;

            if (Mathf.Abs(mvmtDelta.x) > stickDeadzone || Mathf.Abs(mvmtDelta.z) > stickDeadzone)
                currentAnim = runAnim;

            var movementDir = mvmtDelta;
            if (Mathf.Abs(mvmtDelta.x) > stickDeadzone || Mathf.Abs(mvmtDelta.z) > stickDeadzone)
            {
                Vector3 playerPos = playerGraphic.position;
                Vector3 finalPos = playerPos + (camPivot.rotation * movementDir);
                if (!snappingRot) playerGraphic.LookAt(finalPos);
                rigidbody.position = Vector3.MoveTowards(playerPos, finalPos, playerSpeed * Time.deltaTime * 2);
            }
            
            SetGraphicFacing();
            spriteRenderer.sprite = currentAnim.TickAnimFromFacing(playerFacing);
        }

        public void PollMovementInput(InputAction.CallbackContext context)
        {
            if (lockPlayer) return;
            Vector2 readFromEvent = context.ReadValue<Vector2>();
            mvmtDelta = new Vector3(readFromEvent.x,0, readFromEvent.y);
        }

        public void PollCameraRotation(InputAction.CallbackContext context)
        {
            if (lockPlayer) return;
            Vector2 readFromEvent = context.ReadValue<Vector2>();
            float xy = readFromEvent.x;
            if (context.action.activeControl.displayName.Equals("Delta"))
            {
                xy *= mouseSensetivity;
            }
            //xy = Mathf.Clamp(readFromEvent.x, -1, 1);
            camRotDelta = new Vector2(xy, xy);
        }
        
        public void PollInteractKey(InputAction.CallbackContext context)
        {
            if (!context.started) return;
            if (shopping && !lockPlayer)
            {
                FindObjectOfType<ShopManager>().CheckToEnterStore();
            }
            else if (!shopping)
            {
                if (lockPlayer)
                {
                    //This should be the infoPrompt so clear it
                    InfoPrompt.Instance.ClearPrompt();
                }
                else
                {
                    //Check for interactable:s instead
                    FindObjectOfType<DungeonManager>().CheckToEnterDoorTrigger();
                    FindObjectOfType<DungeonManager>().CheckToPickUpGroundItem();
                }
            }
        }

        private void SetGraphicFacing()
        {
            // We set it by comparing if the graphics rotation is within a certain range to set the facing.
            float graphicY = playerGraphic.rotation.eulerAngles.y;
            float cameraY = camPivot.rotation.eulerAngles.y;

            float difference = cameraY - graphicY;
            if (difference >  180) difference -= 360;
            if (difference < -180) difference += 360;
            //This might look a bit off with right and left but it's since we're using the difference, not the actual angle.
            if (difference >  -49 && difference <=   49)      playerFacing = Facing.AWAY;
            else if (difference > -131 && difference <=  -38) playerFacing = Facing.RIGHT;
            else if (difference >   49 && difference <=  131) playerFacing = Facing.LEFT;
            else                                              playerFacing = Facing.TOWARDS;
            
            
            mapPin.rotation = Quaternion.Euler(0, 0, difference);
            if (mapTex != null) mapTex.rotation = Quaternion.Euler(0, 0, cameraY);
        }

        private IEnumerator SnapToBack()
        {
            Vector3 graphicEuler = playerGraphic.rotation.eulerAngles;
            Vector3 camEuler = camPivot.rotation.eulerAngles;
            snappingRot = true;

            camPivot.rotation = Quaternion.Euler(graphicEuler);

            snappingRot = false;
            yield break;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 playerPos = playerGraphic.position;
            Vector3 camPos = camPivot.position;
            Vector3 camForward = camPivot.forward;
            

            if (playerFacing == Facing.TOWARDS)
            {
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0, -49, 0) * camForward) * 0.4f);
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0,  49, 0) * camForward) * 0.4f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0, -131, 0) * camForward) * 0.4f);
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0,  131, 0) * camForward) * 0.4f);
            }
            else if (playerFacing == Facing.LEFT)
            {
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0,  49, 0) * camForward) * 0.4f);
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0,  131, 0) * camForward) * 0.4f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0, -131, 0) * camForward) * 0.4f);
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0, -49, 0) * camForward) * 0.4f);
            }
            else if (playerFacing == Facing.AWAY)
            {
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0, -131, 0) * camForward) * 0.4f);
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0,  131, 0) * camForward) * 0.4f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0, -49, 0) * camForward) * 0.4f);
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0,  49, 0) * camForward) * 0.4f);
            }
            else if (playerFacing == Facing.RIGHT)
            {
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0, -131, 0) * camForward) * 0.4f);
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0, -49, 0) * camForward) * 0.4f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0,  49, 0) * camForward) * 0.4f);
                Gizmos.DrawLine(camPos, camPos + (Quaternion.Euler(0,  131, 0) * camForward) * 0.4f);
            }
            
            
            Gizmos.color = Color.white;
            Gizmos.DrawLine(playerPos, playerPos + playerGraphic.forward * 0.41f);
        }
    }
}
