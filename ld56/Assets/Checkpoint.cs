using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Vector3 respawnPos;

    // Start is called before the first frame update
    void Awake()
    {
        respawnPos = transform.GetChild(0).position;
    }

    public void RememberCheckpoint()
    {
      GameManager.instance.SetLastCheckpoint(respawnPos);
    }
}