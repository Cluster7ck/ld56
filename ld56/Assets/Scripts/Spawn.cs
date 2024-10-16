using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Spawn : MonoBehaviour
{
  [SerializeField] private Ball prefab;

  private Ball current;
  private int n;

  // Start is called before the first frame update
  void Start()
  {
    current = GameObject.Instantiate(prefab);
  }

  // Update is called once per frame
  void Update()
  {
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
      current.Stop();
      n++;
      current = GameObject.Instantiate(prefab);
      current.transform.position = new Vector3(0, 0, -(n * 0.01f));
    }
  }
}