using System.Collections;
using System.Collections.Generic;
using BATTLE;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleNavigator : MonoBehaviour
{
    public Transform boardTransform;
    public GameObject activeTurnCamera;
    private Vector3 activeCamPos;
    public GameObject activeTurnUI;
    public GameObject activeTurnEntity;
    public GameObject turnOverviewCamera;
    private Vector3 overviewCamPos;
    public GameObject turnOverviewEntities;
    private Vector3 boardStartRot;
    public BattleManager bManager;
    [Range(0, 1f)]
    public float rotSpeed;
    public bool spinningBoard = false;
    
    //Add a camera shake module that checks flags[1] for what camera to affect.

    private void Awake()
    {
        boardStartRot = boardTransform.rotation.eulerAngles;
        activeCamPos = activeTurnCamera.transform.position;
        overviewCamPos = turnOverviewCamera.transform.position;
    }

    void Update()
    {
        // Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        // bManager.graphicalInterface.NavigateEntityGrid(movement);
    }

    public void ParseTurn(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (bManager.flags[6]) return; //We're running a timer, don't skip this!
        bManager.ParseTurnState();
    }

    public void RevertTurn(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (bManager.flags[6]) return; //We're running a timer, don't skip this!
        bManager.RevertTurnState();
    }

    public void SubmitPosition(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        bManager.flags[9] = true;
    }

    public void NavigateGrid(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();
        bManager.graphicalInterface.NavigateEntityGrid(movement);
    }

    IEnumerator BoardSpin(float startAngle = 0, float customRotSpeed = 0)
    {
        if (spinningBoard | !bManager.flags[1]) yield break;
        bManager.flags[2] = false;
        Vector3 boardRot = boardStartRot;
        boardTransform.rotation = Quaternion.Euler(boardRot);
        if (customRotSpeed == 0) { customRotSpeed = rotSpeed; }
        if (startAngle > 0) { boardRot.y = startAngle; boardTransform.rotation = Quaternion.Euler(boardRot); }
        spinningBoard = true;

        float yRot = boardRot.y;
        float finalYRot = boardStartRot.y + 360;

        while (yRot <= finalYRot)
        {
            yRot = Mathf.Lerp(yRot, finalYRot + 2, Mathf.Clamp((yRot / finalYRot) * customRotSpeed, 0.01f, 1));
            boardRot.y = yRot;
            boardTransform.rotation = Quaternion.Euler(boardRot);
            yield return new WaitForFixedUpdate();
        }

        boardTransform.rotation = Quaternion.Euler(boardStartRot);
        yield return new WaitForFixedUpdate();

        spinningBoard = false;
        yield break;
    }

    public void SetActionView()
    {
        if (bManager.flags[1]) return;   //We are already at this view!
        activeTurnCamera.SetActive(true);
        turnOverviewCamera.transform.position = overviewCamPos;
        activeTurnUI.SetActive(!bManager.flags[0]);     // Here we check for a EnemyActive flag to see if the menu should be active
        activeTurnEntity.SetActive(true);
        turnOverviewCamera.SetActive(false);
        turnOverviewEntities.SetActive(false);
        spinningBoard = false;
        bManager.flags[1] = true;
        StartCoroutine(BoardSpin(270, 0.06f));
    }

    public void SetOverviewView()
    {
        if (!bManager.flags[1]) return;   //We are already at this view!
        activeTurnCamera.transform.position = activeCamPos;
        turnOverviewCamera.SetActive(true);
        turnOverviewEntities.SetActive(true);
        activeTurnUI.SetActive(false);
        activeTurnEntity.SetActive(false);
        activeTurnCamera.SetActive(false);
        spinningBoard = false;
        //StartCoroutine(BoardSpin(270, 0.06f));
        bManager.flags[1] = false;
        bManager.flags[2] = false;
    }

    public IEnumerator CameraShake(float duration, float max, float min)
    {
        GameObject currentCam = Camera.main.gameObject;
        Vector3 camOriginalPos = currentCam.transform.localPosition;
        while (duration > 0 && currentCam.activeInHierarchy)
        {
            currentCam.transform.localPosition = camOriginalPos + new Vector3(Random.Range(min, max), Random.Range(min / 10, max / 10), 0);

            duration -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        currentCam.transform.localPosition = camOriginalPos;

        yield break;
    }
}
