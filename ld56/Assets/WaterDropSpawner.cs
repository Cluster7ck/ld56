using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDropSpawner : MonoBehaviour
{
  [SerializeField] private GameObject waterDropPrefab;
  [SerializeField] private float timeToSpawn = 2f;

  private float timer = 0f;

  // Update is called once per frame
  void Update()
  {
    timer += Time.deltaTime;
    if (timer >= timeToSpawn)
    {
      Instantiate(waterDropPrefab, transform.position, Quaternion.identity);
      timer = 0f;
    }
  }
}