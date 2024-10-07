using System;
using System.Collections;
using PrimeTween;
using UnityEngine;

public class Collectible : MonoBehaviour
{
  private GameManager gameManager;
  [SerializeField] private float rotDuration;
  [SerializeField] private float bounceDuration;
  [SerializeField] private float scaleDuration;

  [SerializeField] private AudioClip collectClip;

  private float startY;
  
  public void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
    startY = transform.position.y;
  }

  private void Update()
  {
    transform.position = new Vector3(transform.position.x, Mathf.Sin(Time.realtimeSinceStartup).Remap01(0.1f, 0.2f) + startY, 0);
  }

  public void Collect()
  {
    StartCoroutine(CollectAnim());
    AudioManager.Instance.PlaySound(collectClip);
  }

  IEnumerator CollectAnim()
  {
    Destroy(gameObject.GetComponent<Collider2D>());
    gameManager.NumCollectibles += 1;
    var rot = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 720);
    var curRot = transform.eulerAngles;
    var startY = transform.position.y;
    var targetY = transform.position.y + 0.7f;
    yield return Sequence.Create(Tween.EulerAngles(transform, rot, curRot, rotDuration, Ease.InOutQuint))
      .Group(Tween.Scale(transform, Vector3.zero, scaleDuration, Ease.InQuint))
      .Group(Tween.PositionY(transform, targetY, bounceDuration/2, Ease.OutSine))
      .Chain(Tween.PositionY(transform, startY, bounceDuration/2, Ease.OutSine))
      .ToYieldInstruction();
  }
}
