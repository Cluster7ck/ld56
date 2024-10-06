using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private GameManager gameManager;
    private Vector3 respawnPos;

    // Start is called before the first frame update
    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        respawnPos = transform.GetChild(0).position;
    }

    public void RememberCheckpoint()
    {
      gameManager.SetLastCheckpoint(respawnPos);
    }
}
