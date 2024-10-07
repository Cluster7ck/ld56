using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnDemandAudioSource : MonoBehaviour
{
    private bool hasPlayed = false;
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!hasPlayed) hasPlayed = audioSource.isPlaying ? true : false;
        
        if(hasPlayed && !audioSource.isPlaying) {
            Destroy(gameObject);
        }
    }
}
