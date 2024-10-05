using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderArm : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.5f; 
    [SerializeField] private float initMoveSpeed = 0.5f; 
    [SerializeField] private float interval = 2f; 
    [SerializeField] private float initInterval = 2f; 
    [SerializeField] private float rotationRange = 40f;

    private float targetRotation; // Die Zielposition
    private float startRotation; // Startposition für Slerp
    private float timer = 0f; // Timer für das Intervall
    private bool moving = false;
    private float startTime = 0;
    private float initRotation;

    // Start is called before the first frame update
    void Start()
    {
        initRotation = transform.localEulerAngles.z;
    }

    // Update is called once per frame
    void Update()
    {
        ArmRotation();   
    }

    private void ArmRotation() {
        timer += Time.deltaTime;

        if (timer >= interval) {
            startRotation = transform.localEulerAngles.z;
            targetRotation = Random.Range(-rotationRange, rotationRange) + initRotation;
            moveSpeed = initMoveSpeed + Random.Range(-0.3f, 0.3f);
            interval = moveSpeed;
            timer = 0f;
            moving = true;
            startTime = Time.time;
        }

        if (moving) {
            float fracComplete = (Time.time - startTime) / moveSpeed;
            transform.localEulerAngles = Vector3.Slerp(new Vector3(0, 0, startRotation), new Vector3(0, 0, targetRotation), fracComplete);
            if (Mathf.Abs(transform.localEulerAngles.z - targetRotation) < 0.1f) moving = false;
        }
    
    }
}
