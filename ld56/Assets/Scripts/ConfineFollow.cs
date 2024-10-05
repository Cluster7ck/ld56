using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfineFollow : MonoBehaviour
{
    [SerializeField] private GameObject toTrack;
    private float maxX;
    private float prevPosX;
    private float minY;
    
    // Start is called before the first frame update
    void Start()
    {
        maxX = toTrack.transform.position.x;
        minY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        var trackPos = toTrack.transform.position;
        var t = transform;
        t.position += (trackPos - t.position);
        transform.position = new Vector3(t.position.x, Mathf.Min(t.position.y, minY), transform.position.z);
    }
}
