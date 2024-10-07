using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathAnimation : MonoBehaviour
{
    [SerializeField] private float distance = 0f;
    [SerializeField] private float duration = 0f;
    [SerializeField] private AnimationCurve floatCurve;
    [SerializeField] private AnimationCurve scaleCurve;

    private float timer = 0f;
    private Vector3 initPos;
    private Vector3 initScale;

    private void Start () {
        initPos = transform.position;
        initScale = transform.localScale;
    }
    // Update is called once per frame
    void Update() 
    {
        float yPos = initPos.y + floatCurve.Evaluate(timer/duration) * distance ;
        transform.localPosition = new Vector3(initPos.x + 0.5f, yPos, initPos.z);
        transform.localScale = initScale * scaleCurve.Evaluate(timer/duration);
        timer += Time.deltaTime * Time.timeScale;
        if(timer > duration) {
            Destroy(this.gameObject);
        }
    }
}