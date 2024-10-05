using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private List<GameObject> layers;

    private Camera camera;
    // Start is called before the first frame update
    void Start()
    {
        camera = Camera.main;
        int i = 0;
        foreach (Transform child in transform)
        {
            layers.Add(child.gameObject);
            var spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = i * 10;
            }

            var layerChildren = child.GetComponentsInChildren<SpriteRenderer>();
            foreach (var rend in layerChildren)
            {
                rend.sortingOrder = i * 10;
            }

            i++;
        }
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
