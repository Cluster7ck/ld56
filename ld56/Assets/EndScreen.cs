using System;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void Restart()
    {
        Tween.StopAll();
        SceneManager.LoadScene(0);
    }
}
