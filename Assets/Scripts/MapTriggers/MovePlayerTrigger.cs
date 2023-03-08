using MOVEMENT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovePlayerTrigger : MonoBehaviour
{
    //We need a queue of "moves" that the player will make.
    //Simple things like turning to a certain facing and walking forward/back/left/right.
    //Also have a fallback teleport that happens after everything is done. If we landed where we want, it should be imperceptible.


    public UnityEvent onFinished = new UnityEvent();
    public Vector3 targetPos = Vector3.zero;
    public Vector3 targetRot = Vector3.zero;
    public List<Move> moves = new List<Move>();

    private FPSGridPlayer player;
    private bool triggered = false;

    private void Start()
    {
        player = FindObjectOfType<FPSGridPlayer>();
    }

    public void Trigger()
    {
        if (triggered) return;
        triggered = true;

        StartCoroutine(IEMovePlayer());
    }

    public IEnumerator IEMovePlayer()
    {
        player.AddLock("MOVE_PLAYER_TRIGGER");

        Queue<Move> moveList = new Queue<Move>(moves);
        while(moveList.Count > 0)
        {
            Move current = moveList.Dequeue();

            switch(current.moveType)
            {
                case Move.MoveTypes.Turn:
                {
                    player.LookAt(current.facing);
                    break;
                }

                case Move.MoveTypes.Walk_Forward:  { player.MovePlayerExt(Vector2.up);    break; }
                case Move.MoveTypes.Walk_Backward: { player.MovePlayerExt(Vector2.down);  break; }
                case Move.MoveTypes.Walk_Left:     { player.MovePlayerExt(Vector2.left);  break; }
                case Move.MoveTypes.Walk_Right:    { player.MovePlayerExt(Vector2.right); break; }
            }

            yield return new WaitUntil(() => (!player.IsMoving && !player.IsTurning));
            yield return new WaitForSecondsRealtime(0.15f); // just a small dampener, it was too fast
        }

        player.transform.position = targetPos;
        player.transform.rotation = Quaternion.Euler(targetRot);

        onFinished?.Invoke();
        player.RemoveLock("MOVE_PLAYER_TRIGGER");
        yield return null;
    }


    [System.Serializable]
    public struct Move
    {
        public MoveTypes moveType;
        public MinimapCompass.Facing facing;

        public enum MoveTypes
        {
            Walk_Forward,
            Walk_Backward,
            Walk_Left,
            Walk_Right,
            Turn
        }
    }
}
