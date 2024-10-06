using UnityEngine;
using UnityEngine.Serialization;

public class CollisionStateTransition : MonoBehaviour
{
  public bool doTransition = true;
  [FormerlySerializedAs("TransitionToFromTop")] [SerializeField] private State transitionToFromTop = State.WaitInput;
  [FormerlySerializedAs("TransitionToFromSide")] [SerializeField] private State transitionToFromSide = State.Falling;

  public State? TransitionToFromTop => doTransition ? transitionToFromTop : null;
  public State? TransitionToFromSide => doTransition ? transitionToFromSide : null;
}