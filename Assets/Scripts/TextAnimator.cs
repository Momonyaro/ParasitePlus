using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class TextAnimator : MonoBehaviour
{
    public TextMeshProUGUI textElement;
    public TextMeshProUGUI[] textSlaves;

    [Header("Random Capitalization")]
    public bool randomCapitalization = false;
    [Range(0, 1)] public float randCapChance = 0.5f;
    public float randCapTimer = 0.1f;
    private float _randCapTimer = 0.0f;
    public float randCapMinMaxRandOffset = 0.00f;

    [Header("Blinking Text")]
    public bool blinkingText = false;
    public float visibleFor = 0.2f;
    public float invisibleFor = 0.1f;
    private bool _visible = false;
    private float _visibleTimer = 0.0f;



    private void Awake()
    {
        _randCapTimer = randCapTimer + Random.Range(-randCapMinMaxRandOffset, randCapMinMaxRandOffset);
    }

    private void Update()
    {
        string currentText = textElement.text;
        
        if (randomCapitalization)
            RandomCapitalization(ref currentText);

        if (blinkingText)
            BlinkingText(textElement);

        textElement.text = currentText;

        for (int i = 0; i < textSlaves.Length; i++)
        {
            textSlaves[i].text = currentText;
        }
    }

    private void RandomCapitalization(ref string currentText)
    {
        _randCapTimer -= Time.deltaTime;
        if (_randCapTimer <= 0)
        {
            _randCapTimer = randCapTimer + Random.Range(-randCapMinMaxRandOffset, randCapMinMaxRandOffset);
            char[] txtArray = currentText.ToCharArray();

            for (int i = 0; i < txtArray.Length; i++)
            {
                txtArray[i] = (Random.value >= randCapChance) ? char.ToUpper(txtArray[i]) : char.ToLower(txtArray[i]);
            }

            currentText = new string(txtArray);
        }
    }

    private void BlinkingText(TextMeshProUGUI textComponent)
    {
        if (_visibleTimer <= 0)
        {
            _visible = !_visible;
            _visibleTimer = (_visible) ? visibleFor : invisibleFor;
            textComponent.enabled = _visible;
        }

        _visibleTimer -= Time.deltaTime;
    }
}
