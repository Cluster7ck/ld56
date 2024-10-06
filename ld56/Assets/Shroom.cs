using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shroom : MonoBehaviour
{
  [SerializeField] private Animator animator;
  public float BounceStrength;

  public void DoBounce()
  {
    animator.SetTrigger("jumpTrigger");
  }
}