using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Start,
    Tutorial,
    Story,
    Playing,
    Paused,
    Died,
    Win
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameState currentState;

    [SerializeField] private Cricket player;
    [SerializeField] private GameObject virtualCamera;

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

    void Start()
    {
        // Initialer Zustand
        ChangeState(currentState);
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        switch (currentState)
        {
            case GameState.Start:
                UIManager.instance.ZoomToStart(virtualCamera);
                UIManager.instance.StartScreen.SetActive(true);
                player.enabled = false;
                break;
            case GameState.Tutorial:

                break;
            case GameState.Story:

                break;
            case GameState.Playing:
                UIManager.instance.ZoomToPlay(virtualCamera);
                Time.timeScale = 1f;
                player.enabled = true;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                UIManager.instance.PauseScreen.SetActive(true);
                player.enabled = false;
                break;
            case GameState.Died:
                
                break;
            case GameState.Win:
                UIManager.instance.EndScreen.SetActive(true);
                break;
        }

        if(currentState != GameState.Start) {
            UIManager.instance.StartScreen.SetActive(false);
        }
        if(currentState != GameState.Paused) {
            //UIManager.instance.PauseScreen.SetActive(false);
        }
        if(currentState != GameState.Win) {
            UIManager.instance.EndScreen.SetActive(false);
        }
    }

    private void Update () {
        
    }

    public void StartGame() {
        ChangeState(GameState.Playing);
    }

    public void ReachedGoal() {
        ChangeState(GameState.Win);
    }
}