using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    public Animator animator;
    public string animationName;

    public void TriggerEvent()
    {
        animator.Play(animationName);
    }
}
