using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BadTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"trigger {other.gameObject.name}");
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        Debug.Log($"collision {other.gameObject.name}");
    }
}
