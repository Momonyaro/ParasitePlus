using System;
using System.Collections;
using System.Collections.Generic;
using MOVEMENT;
using UnityEngine;
using Random = UnityEngine.Random;

public class Pedestrian : MonoBehaviour
{
    [Range(0, 1)]public float minR = .5f;
    [Range(0, 1)]public float minG = .5f;
    [Range(0, 1)]public float minB = .5f;
    private Color bodyColor = Color.white;
    [Min(0.1f)] public float maxPlayerDist = 1.2f;
    public float minPlayerDist = 1.2f;
    
    [Header("Body Parts")] 
    public Sprite[] heads = new Sprite[0];
    public Sprite[] expressions = new Sprite[0];
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer headRenderer;
    public SpriteRenderer exprRenderer;
    public SpriteRenderer shadowRenderer;

    private PlayerMovement player;

    private void Awake()
    {
        player = FindObjectOfType<PlayerMovement>();
        
        bodyColor = new Color(Random.Range(minR, 1), Random.Range(minG, 1), Random.Range(minB, 1));
        ApplyColorToPed(bodyColor);
        GenerateRandomPed();
    }

    private void Update()
    {
        ModifyAlphaOnDistance(Vector3.Distance(transform.position, player.camPivot.position));
    }

    private void GenerateRandomPed()
    {
        headRenderer.sprite = heads[Random.Range(0, heads.Length)];
        exprRenderer.sprite = expressions[Random.Range(0, expressions.Length)];
    }
    
    private void ApplyColorToPed(Color newCol)
    {
        bodyRenderer.color = newCol;
        headRenderer.color = newCol;
        exprRenderer.color = new Color(1, 1, 1, newCol.a);
        shadowRenderer.color = new Color(1, 1, 1, newCol.a);
    }

    private void ModifyAlphaOnDistance(float distance)
    {
        Color faded = bodyColor;
        
        if (distance < maxPlayerDist + minPlayerDist)
        {
            distance -= minPlayerDist;
            distance = Mathf.Clamp(distance, 0, 100);
            
            faded.a = distance / maxPlayerDist;
        }
        
        ApplyColorToPed(faded);
    }
}
