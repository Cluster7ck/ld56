using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OnResetEvent : UnityEvent
{
}

public class Resettable : MonoBehaviour
{
  public OnResetEvent OnReset;
}