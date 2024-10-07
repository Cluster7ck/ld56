using System;
using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class WaterDrop : MonoBehaviour
{
  [SerializeField] private float startSize = 0.5f;
  [SerializeField] private float stepSize = 0.5f;

  [SerializeField] private float stepTime = 1f;
  [SerializeField] private int stepsUntilDrop = 3;

  [SerializeField] private float shakeStrength = 2.0f;

  [SerializeField] private ParticleSystem particleSystem;

  private Rigidbody2D rb;

  private int playerLayer;
  private int groundLayer;
  private bool dead = false;

  // Start is called before the first frame update
  void Start()
  {
    groundLayer = LayerMask.NameToLayer("Ground");
    playerLayer = LayerMask.NameToLayer("Character");
    rb = GetComponent<Rigidbody2D>();
    transform.localScale = new Vector3(startSize, startSize, startSize);
    StartCoroutine(StartDropping());
  }

  IEnumerator StartDropping()
  {
    float endScale = stepSize * stepsUntilDrop;
    float duration = stepTime * stepsUntilDrop;
    float shakeTime = duration / 3;
    float shakeStartTime = duration - shakeTime;

    yield return Sequence.Create(Tween.Scale(transform, endScale, duration, Ease.Linear))
      .Insert(shakeStartTime, Tween.ShakeLocalPosition(transform, new Vector3(-shakeStrength, 0, 0), shakeTime, 10F, false))
      .ToYieldInstruction();

    rb.bodyType = RigidbodyType2D.Dynamic;
  }


  IEnumerator Die()
  {
      rb.bodyType = RigidbodyType2D.Static;
      particleSystem.gameObject.SetActive(true);
      yield return Tween.Scale(transform, Vector3.zero, particleSystem.main.duration/2f);
      yield return new WaitForSeconds(particleSystem.main.duration / 2f);
      Destroy(gameObject);
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.gameObject.layer == groundLayer)
    {
      StartCoroutine(Die());
    }

    if (other.gameObject.layer == playerLayer)
    {
      other.gameObject.GetComponent<Cricket>().gameManager.ChangeState(GameState.Died);
    }
  }
}