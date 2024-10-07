using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shroom : MonoBehaviour
{
  [SerializeField] private Animator animator;
  [SerializeField] private AudioClip bounceClip;
  public float BounceStrength;


  public void DoBounce()
  {
    animator.SetTrigger("jumpTrigger");
    AudioManager.Instance.PlaySound(bounceClip, BounceStrength * 0.01f - 0.2f);
  }
}