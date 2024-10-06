using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class Collectible : MonoBehaviour
{
  private GameManager gameManager;
  [SerializeField] private float rotDuration;
  [SerializeField] private float bounceDuration;
  [SerializeField] private float scaleDuration;
  
  public void Awake()
  {
    gameManager = FindObjectOfType<GameManager>();
  }
  
  public void Collect()
  {
    StartCoroutine(CollectAnim());
  }

  IEnumerator CollectAnim()
  {
    var rot = new Vector3(transform.eulerAngles.x, 1440, transform.eulerAngles.z);
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
