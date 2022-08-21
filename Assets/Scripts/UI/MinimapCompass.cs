using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCompass : MonoBehaviour
{
    public enum Facing
    {
        NORTH,
        SOUTH,
        WEST,
        EAST,
    }

    public RectTransform north;
    public RectTransform south;
    public RectTransform east;
    public RectTransform west;

    public AnimationCurve lerpCurve;
    public Facing facing;

    private Vector2 nPos;
    private Vector2 sPos;
    private Vector2 ePos;
    private Vector2 wPos;

    private void Awake()
    {
        nPos = north.anchoredPosition;
        sPos = south.anchoredPosition;
        ePos = east.anchoredPosition;
        wPos = west.anchoredPosition;
    }

    private IEnumerator ReorderCompass()
    {
        Vector2 newN = ReorderDirection(Facing.NORTH, facing);
        Vector2 newS = ReorderDirection(Facing.SOUTH, facing);
        Vector2 newE = ReorderDirection(Facing.EAST, facing);
        Vector2 newW = ReorderDirection(Facing.WEST, facing);

        Vector2 startN = north.anchoredPosition;
        Vector2 startS = south.anchoredPosition;
        Vector2 startE = east.anchoredPosition;
        Vector2 startW = west.anchoredPosition;

        float timer = 0;
        float maxTimer = lerpCurve.keys[lerpCurve.keys.Length - 1].time;

        while (timer < maxTimer)
        {
            north.anchoredPosition = Vector2.Lerp(startN, newN, lerpCurve.Evaluate(timer));
            south.anchoredPosition = Vector2.Lerp(startS, newS, lerpCurve.Evaluate(timer));
            east.anchoredPosition = Vector2.Lerp(startE, newE, lerpCurve.Evaluate(timer));
            west.anchoredPosition = Vector2.Lerp(startW, newW, lerpCurve.Evaluate(timer));

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        north.anchoredPosition = Vector2.Lerp(startN, newN, lerpCurve.Evaluate(maxTimer));
        south.anchoredPosition = Vector2.Lerp(startS, newS, lerpCurve.Evaluate(maxTimer));
        east.anchoredPosition = Vector2.Lerp(startE, newE, lerpCurve.Evaluate(maxTimer));
        west.anchoredPosition = Vector2.Lerp(startW, newW, lerpCurve.Evaluate(maxTimer));

        yield break;
    }

    private Vector2 ReorderDirection(Facing absolute, Facing current)
    {
        switch (absolute)
        {
            case Facing.NORTH:
                switch(current) {
                    case Facing.NORTH: return nPos;
                    case Facing.SOUTH: return sPos;
                    case Facing.EAST: return ePos;
                    case Facing.WEST: return wPos;
                    default: return Vector2.zero;
                }

            case Facing.SOUTH:
                switch (current) {
                    case Facing.NORTH: return sPos;
                    case Facing.SOUTH: return nPos;
                    case Facing.EAST: return wPos;
                    case Facing.WEST: return ePos;
                    default: return Vector2.zero;
                }

            case Facing.WEST:
                switch (current) {
                    case Facing.NORTH: return wPos;
                    case Facing.SOUTH: return ePos;
                    case Facing.EAST: return nPos;
                    case Facing.WEST: return sPos;
                    default: return Vector2.zero;
                }

            case Facing.EAST:
                switch (current) {
                    case Facing.NORTH: return ePos;
                    case Facing.SOUTH: return wPos;
                    case Facing.EAST: return sPos;
                    case Facing.WEST: return nPos;
                    default: return Vector2.zero; }

            default: return Vector2.zero;
        }
    }

    public void SetCompassFacing(Facing facing)
    {
        this.facing = facing;

        StopAllCoroutines();
        StartCoroutine(ReorderCompass());
    }
}
