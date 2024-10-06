using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class transferspritesize : MonoBehaviour
{
    private SpriteRenderer parentSpriteRenderer;
    private SpriteRenderer childSpriteRenderer;

    private void OnEnable()
    {
        parentSpriteRenderer = GetComponent<SpriteRenderer>();
        childSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (parentSpriteRenderer.size != childSpriteRenderer.size)
        {
            childSpriteRenderer.size = parentSpriteRenderer.size;
        }
    }
}
