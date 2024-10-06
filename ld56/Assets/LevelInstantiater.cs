using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelChunkInstantiater : MonoBehaviour
{
    [SerializeField] private List<GameObject> levelChunks = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject chunk in levelChunks) {
            Instantiate(chunk, transform, false);
        }
    }

}
