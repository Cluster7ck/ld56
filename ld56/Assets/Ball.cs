using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ball : MonoBehaviour
{
    [SerializeField] private float growSpeed;
    [SerializeField] private Vector2 h;
    [SerializeField] private Vector2 s;
    [SerializeField] private Vector2 v;
    private Transform trans;
    private bool active = true;

    private void Awake()
    {
        trans = transform;
        var renderer = GetComponent<SpriteRenderer>();
        renderer.color = Random.ColorHSV(h.x, h.y, s.x, s.y, v.x, v.y);
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            trans.localScale += Vector3.one * (growSpeed * Time.deltaTime);
        }
    }

    public void Stop()
    {
        active = false;
    }
}
