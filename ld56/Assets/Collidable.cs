using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OnCollision : UnityEvent<Cricket>{}
public class Collidable : MonoBehaviour
{
    public OnCollision OnCollision;

    public void Collide(Cricket cricket)
    {
        OnCollision.Invoke(cricket);
    }
}