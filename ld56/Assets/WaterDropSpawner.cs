using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDropSpawner : MonoBehaviour
{
    [SerializeField] private GameObject waterDropPrefab;
    [SerializeField] private float timeToSpawn = 2f;

    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.instance.currentState == GameState.Playing) {
            timer += Time.deltaTime;
            if(timer >= timeToSpawn) {
                
                Instantiate(waterDropPrefab, transform, false);
                timer = 0f;
            }
        }
    }
}
