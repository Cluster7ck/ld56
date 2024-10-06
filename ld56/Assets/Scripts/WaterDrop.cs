using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDrop : MonoBehaviour
{
    [SerializeField] private float startSize = 0.5f;
    [SerializeField] private float stepSize = 0.5f;
    
    [SerializeField] private float stepTime = 1f;
    [SerializeField] private int stepsUntilDrop = 3;

    private float timer = 0f;
    private int stepCount = 0;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(startSize, startSize, startSize);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if(stepCount >= stepsUntilDrop) {
            rb.bodyType = RigidbodyType2D.Dynamic;
            
        } else {
            if(timer >= stepTime) {
                stepCount++;
                transform.localScale = new Vector3(transform.localScale.x + stepSize, transform.localScale.y + stepSize, transform.localScale.z + stepSize);
                timer = 0;
            }
        }
    }
}
