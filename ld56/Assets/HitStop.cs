using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class HitStop : MonoBehaviour
{
  [SerializeField] private float bulletTimeScale;
  [SerializeField] private AnimationCurve bulletTimeCurve;
  private bool waiting;

  private void OnDisable()
  {
    StopAllCoroutines();
  }

  public void Stop(float duration)
  {
    if (waiting) return;
    waiting = true;
    StartCoroutine(DoHitStop(duration));
  }

  public void BulletTime(float duration)
  {
    Debug.Log("BulletTime");
    if (waiting) return;
    waiting = true;
    StartCoroutine(DoBulletTime(duration));
  }

  IEnumerator DoHitStop(float duration)
  {
    Time.timeScale = 0;
    yield return new WaitForSecondsRealtime(duration);
    Time.timeScale = 1;
    waiting = false;
  }

  IEnumerator DoBulletTime(float duration)
  {
    yield return Tween.GlobalTimeScale(bulletTimeScale, duration/2, Ease.OutQuart).ToYieldInstruction();
    yield return Tween.GlobalTimeScale(1, duration/2, Ease.InQuart).ToYieldInstruction();
    Time.timeScale = 1;
    waiting = false;
  }
}