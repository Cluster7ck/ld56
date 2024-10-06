using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathAnimation : MonoBehaviour
{
    [SerializeField] private float distance = 0f;
    [SerializeField] private float duration = 0f;
    [SerializeField] private AnimationCurve floatCurve;

    private float timer = 0f;
    private Vector3 initPos;

    private void Start () {
        initPos = transform.position;
    }
    // Update is called once per frame
    void Update() 
    {
        float yPos = initPos.y + floatCurve.Evaluate(timer/duration) * distance ;
        transform.localPosition = new Vector3(transform.localPosition.x, yPos, transform.localPosition.z);
        timer += Time.deltaTime * Time.timeScale;
        if(timer > duration) {
            Destroy(this.gameObject);
        }
    }
}