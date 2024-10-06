using UnityEngine;
using UnityEngine.Events;

public class OnCollision : UnityEvent<Cricket>{}
public class Collidable : MonoBehaviour
{
    public OnCollision OnCollision;

    public void Collide(Cricket cricket)
    {
        OnCollision.Invoke(cricket);
    }
}