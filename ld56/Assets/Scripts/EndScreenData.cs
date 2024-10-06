using UnityEngine;
using UnityEngine.Serialization;

public class EndScreenData : MonoBehaviour
{
  public int MaxNumCollectibles;
  public int NumCollectibles;
  [FormerlySerializedAs("time")] public float ElapsedTime;
}