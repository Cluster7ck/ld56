using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D ( Collider2D collision ) {
        Debug.Log("Collided with " + collision.name);
        if (collision.gameObject.layer == LayerMask.NameToLayer("Character")) {
            Debug.Log("Collided with Character yay");
            GameManager.instance.ReachedGoal();
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        
    }
}
