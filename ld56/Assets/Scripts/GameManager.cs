using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
  [SerializeField] private AudioSource audioSource;
    
  [SerializeField] private Animator animator;
  private Resettable[] resettables;

  private int maxNumCollectibles;
  private int numCollectibles = 0;
  private float elapsedTime;

  private Vector3 playerStartPos;

  private bool _muteState = false;

  public int NumCollectibles
  {
    get => numCollectibles;
    set
    {
      numCollectibles = value;
      UIManager.instance.SetNumCollectibles(numCollectibles, maxNumCollectibles);
    }
  }

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
    maxNumCollectibles = FindObjectsByType<Collectible>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
    if (!audioSource)
    {
      audioSource = Camera.main.GetComponentInChildren<AudioSource>();
    }
  }

  void Start()
  {
    // Initialer Zustand
    ChangeState(currentState);
    playerStartPos = player.transform.position;
    Debug.Log("Saved Player Start Position: " + playerStartPos);
  }

  public void ToggleMute()
  {
    // TODO toggle button icon
    audioSource.mute = !audioSource.mute;
    _muteState = audioSource.mute;
    UIManager.instance.SetMuteButtonText(audioSource.mute);
  }

  public bool muteState => _muteState;

  public void ChangeState(GameState newState)
  {
    currentState = newState;

    switch (currentState)
    {
      case GameState.Start:
        StartCoroutine(StartSequence());
        break;
      case GameState.Playing:
        Time.timeScale = 1f;
        UIManager.instance.SetNumCollectibles(numCollectibles, maxNumCollectibles);
        UIManager.instance.PauseScreen.SetActive(false);
        UIManager.instance.PlayScreen.SetActive(true);
        player.enabled = true;
        break;
      case GameState.Paused:
        Time.timeScale = 0f;
        UIManager.instance.PauseScreen.SetActive(true);
        player.enabled = false;
        break;
      case GameState.Died:
        Die();
        UIManager.instance.DeathScreen.SetActive(true);
        player.Animator.SetBool("isDead", true);
                
        break;
      case GameState.Win:
        var go = new GameObject();
        var esd = go.AddComponent<EndScreenData>();
        esd.NumCollectibles = numCollectibles;
        esd.ElapsedTime = elapsedTime;
        esd.MaxNumCollectibles = maxNumCollectibles;
        DontDestroyOnLoad(go);
        Tween.StopAll();
        SceneManager.LoadScene(1);
        //UIManager.instance.EndScreen.SetActive(true);
        break;
    }

    if (currentState != GameState.Paused)
    {
      //UIManager.instance.PauseScreen.SetActive(false);
    }

    if (currentState != GameState.Died)
    {
      UIManager.instance.DeathScreen.SetActive(false);
      player.Animator.SetBool("isDead", false);
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

      elapsedTime += Time.deltaTime;
    }

    if ((currentState == GameState.Paused || currentState == GameState.Playing) &&
        (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.P)))
    {
      if (currentState != GameState.Paused)
      {
        ChangeState(GameState.Paused);
      }
      else
      {
        ChangeState(GameState.Playing);
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

    // wait for first click
    while (!Mouse.current.leftButton.wasReleasedThisFrame)
    {
      yield return null;
    }

    yield return new WaitForSeconds(3.0f);
    Destroy(startAnim);
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
  // stimmt, aber ich wusste nicht so schnell wie und hab noch komische bools hinzugef�gt
  private void Die()
  {
    player.Die(lastCheckpoint);
  }

  private void ResetResettables()
  {
    foreach (var resettable in resettables) // Tische zur�cksetzen? :P
    {
      resettable.OnReset.Invoke();
    }
  }

  private void SetPlayerToLastCheckpoint()
  {
    player.transform.position = lastCheckpoint;
    player.TransitionToFalling();
  }

  private Vector3 lastCheckpoint;

  public void SetLastCheckpoint(Vector3 respawnPos)
  {
    if (respawnPos.x > lastCheckpoint.x)
    {
      lastCheckpoint = respawnPos;
    }
  }

  public void ResetToLastCheckpoint()
  {
    Debug.Log("Resetting to checkpoint");
    player.gameObject.SetActive(true);
    SetPlayerToLastCheckpoint();
    ResetResettables();
    ChangeState(GameState.Playing);
  }


  public void ResetToStart()
  {
    SceneManager.LoadScene(0);
  }
}