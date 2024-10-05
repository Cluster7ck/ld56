using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private GameObject endScreen;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject StartScreen {
        get { return startScreen; }
    }

    public GameObject PauseScreen {
        get { return pauseScreen; }
    }

    public GameObject EndScreen {
        get { return endScreen; }
    }

}