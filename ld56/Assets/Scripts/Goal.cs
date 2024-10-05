using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    int characterLayer;
    // Start is called before the first frame update
    void Start()
    {
        characterLayer = LayerMask.NameToLayer("Character");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D ( Collider2D collision ) {
        if (collision.gameObject.layer == characterLayer) {
            GameManager.instance.ReachedGoal();
        }
    }
}
