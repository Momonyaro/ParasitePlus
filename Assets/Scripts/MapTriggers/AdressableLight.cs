using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdressableLight : MonoBehaviour
{
    public Renderer lightRenderer;
    public string baseColorName = "_BaseColor";
    public string emissiveColorName = "_BaseColor";

    public void SetLightColor(Color color, Color emission)
    {
        Material material = lightRenderer.material;

        material.SetColor(baseColorName, color);
        material.SetColor(emissiveColorName, emission);

        lightRenderer.material = material;
    }
}
