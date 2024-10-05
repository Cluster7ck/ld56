using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingAlongAxis : MonoBehaviour
{
    [SerializeField] private bool x;
    private bool pingX = true;
    [SerializeField] private bool y;
    private bool pingY = true;

    [SerializeField] private float xSpeed;
    [SerializeField] private float ySpeed;
    [SerializeField] private float xRange;
    [SerializeField] private float yRange;

    private Vector2 initPos;

    // Start is called before the first frame update
    void Start()
    {
        initPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float newX = transform.position.x;
        if (x) {
            if (pingX) {
                newX += xSpeed * Time.deltaTime;
            } else {
                newX -= xSpeed * Time.deltaTime;
            }
            if ((newX < initPos.x - xRange) || (newX > initPos.x + xRange)) { 
                newX = Mathf.Clamp(newX, initPos.x - xRange, initPos.x + xRange);
                pingX = !pingX;
            }
        }
        
        float newY = transform.position.y;
        if(y) {
            if(pingY) {
                newY += ySpeed * Time.deltaTime;
            } else {
                newY -= ySpeed * Time.deltaTime;
            }
            if ((newY < initPos.y - yRange) || (newY > initPos.y + yRange)) { 
                newY = Mathf.Clamp(newY, initPos.y - yRange, initPos.y + yRange);
                pingY = !pingY;
            }
        }
        transform.position = new Vector2(newX, newY);
    }
}
