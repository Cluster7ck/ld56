using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
  Start,
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

  [SerializeField] private GameObject startAnimation;

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
    UIManager.instance.StartScreen.SetActive(false);
    ChangeState(currentState);
  }

  public void ChangeState(GameState newState)
  {
    currentState = newState;

    switch (currentState)
    {
      case GameState.Start:
        StartCoroutine(StartSequence());
        break;
      case GameState.Playing:
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

    if (currentState != GameState.Paused)
    {
      //UIManager.instance.PauseScreen.SetActive(false);
    }

    if (currentState != GameState.Win)
    {
      UIManager.instance.EndScreen.SetActive(false);
    }
  }

  private void Update()
  {
    if (currentState == GameState.Playing)
    {
      if (player.jumpState == State.PrepareJump || player.jumpState == State.BulletTimePrepareJump ||
          player.jumpState == State.JumpingDown || player.jumpState == State.JumpingUp || player.jumpState == State.Falling ||
          player.jumpState == State.DoubleJumping || player.jumpState == State.BulletTimeWaitInput || player.jumpState == State.Bounce)
      {
        UIManager.instance.ZoomToJump(virtualCamera);
      }
      else
      {
        UIManager.instance.ZoomToPlay(virtualCamera);
      }
    }
  }

  private IEnumerator StartSequence()
  {
    player.enabled = false;
    UIManager.instance.InstaZoomToStart(virtualCamera);
    var startAnim = Instantiate(startAnimation);
    yield return new WaitForSeconds(1f);
    yield return UIManager.instance.ZoomToPlayCo(virtualCamera);
    
    ChangeState(GameState.Playing);
    player.enabled = true;
    
    // wait for first click
    while (!Mouse.current.leftButton.wasReleasedThisFrame)
    {
      yield return null;
    }

    yield return new WaitForSeconds(3.0f);
    Destroy(startAnim);
    Time.timeScale = 1f;
  }

  public void StartGame()
  {
    ChangeState(GameState.Playing);
  }

  public void ReachedGoal()
  {
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