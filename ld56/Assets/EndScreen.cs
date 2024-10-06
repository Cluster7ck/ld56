using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndScreen : MonoBehaviour
{
    [SerializeField] private TMP_Text collectiblesText;
    [SerializeField] private TMP_Text time;
    // Start is called before the first frame update
    void Start()
    {
        var esd = FindObjectOfType<EndScreenData>();
        if (esd)
        {
            collectiblesText.text = $"{esd.NumCollectibles}/{esd.MaxNumCollectibles}";
            time.text = $"{TimeSpan.FromSeconds(Mathf.RoundToInt(esd.ElapsedTime)):c}";
        }
    }
}
