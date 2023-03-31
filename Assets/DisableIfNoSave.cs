using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableIfNoSave : MonoBehaviour
{
    private void Awake()
    {
        if (!SaveUtility.SaveExists())
            gameObject.SetActive(false);
    }
}
