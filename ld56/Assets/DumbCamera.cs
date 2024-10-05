using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbCamera : MonoBehaviour
{
    [SerializeField] private GameObject toTrack;
    private float maxX;
    
    // Start is called before the first frame update
    void Start()
    {
        maxX = transform.position.x;
    }

    // Update is called once per frame
    void Update()
    {
        var trackX = toTrack.transform.position.x;
        if (trackX > maxX)
        {
            this.transform.position += Vector3.right * (trackX - maxX);
            maxX = trackX;
        }
    }
}
