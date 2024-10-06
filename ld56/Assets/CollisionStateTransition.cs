using UnityEngine;

public class CollisionStateTransition : MonoBehaviour
{
  public State TransitionToFromTop = State.WaitInput;
  public State TransitionToFromSide = State.Falling;
}