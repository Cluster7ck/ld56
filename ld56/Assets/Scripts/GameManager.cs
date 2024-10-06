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

    private Resettable[] resettables;

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

        resettables = FindObjectsByType<Resettable>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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
                Die();
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
        if(currentState == GameState.Playing) {
            if(player.jumpState == State.PrepareJump || player.jumpState == State.BulletTimePrepareJump || player.jumpState == State.JumpingDown || player.jumpState == State.JumpingUp || player.jumpState == State.Falling || player.jumpState == State.DoubleJumping || player.jumpState == State.BulletTimeWaitInput || player.jumpState == State.Bounce)  {
                UIManager.instance.ZoomToJump(virtualCamera);
            } else {
                UIManager.instance.ZoomToPlay(virtualCamera);
            }
        }
    }

    public void StartGame() {
        ChangeState(GameState.Playing);
    }

    public void ReachedGoal() {
        ChangeState(GameState.Win);
    }

    // TODO should probably be a coroutine
    private void Die()
    {
        // play death animation
        // Reset all resettables
        player.transform.position = lastCheckpoint;
        player.SetState(State.Falling);
        foreach (var resettable in resettables)
        {
            resettable.OnReset.Invoke();
        }
    }

    private Vector3 lastCheckpoint;
    public void SetLastCheckpoint(Vector3 respawnPos)
    {
        if (respawnPos.x > lastCheckpoint.x)
        {
            lastCheckpoint = respawnPos;
        }
    }
}