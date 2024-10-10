using System.Collections;
using UnityEngine;
using PrimeTween;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

[SerializeField]
public class CameraSceneParameters
{
  public CameraSceneParameters(float pOrthographicSize, float pXcameraOffset, float pYcameraOffset)
  {
    orthographicSize = pOrthographicSize;
    cameraOffset = new Vector2(pXcameraOffset, pYcameraOffset);
  }

  public float orthographicSize;
  public Vector2 cameraOffset; //offset relative to player
}

public class UIManager : MonoBehaviour
{
  public static UIManager instance;

  [FormerlySerializedAs("startScreen")] [SerializeField] private GameObject playScreen;
  [SerializeField] private GameObject pauseScreen;
  [SerializeField] private GameObject deathScreen;
  [SerializeField] private GameObject endScreen;

  [SerializeField] private CameraSceneParameters cameraGameParameters = new CameraSceneParameters(12f, 7f, 7f);
  [SerializeField] private CameraSceneParameters cameraStartParameters = new CameraSceneParameters(2f, 1.2f, 0.5f);


  private const float cameraZoomTime = 2f;

  [Header("Game Camera")] [SerializeField]
  private float gameCameraSize = 4f;

  [SerializeField] private Vector2 gameCameraOffset = new Vector2(3f, 2f);

  //CameraSettings when charging Jump
  [Header("Jump Charge Camera")] [SerializeField]
  private float jumpZoomTime = 2f;

  [SerializeField] private float jumpCameraSize = 4f;
  [SerializeField] private Vector2 jumpCameraOffset = new Vector2(3f, 2f);

  [FormerlySerializedAs("text")] [Header("Ui")] [SerializeField] private TMP_Text collectiblesNumText;
  [SerializeField] private TMP_Text muteText;
  [SerializeField] public Slider slider;
  
  public GameObject PlayScreen => playScreen;
  public GameObject PauseScreen => pauseScreen;
  public GameObject DeathScreen => deathScreen;
  public GameObject EndScreen => endScreen;


  void Awake()
  {
    if (instance == null)
    {
      instance = this;
    }
    else
    {
      Destroy(gameObject);
    }
  }

  private void Start()
  {
    cameraGameParameters.orthographicSize = gameCameraSize;
    cameraGameParameters.cameraOffset = gameCameraOffset;
  }

  public void SetMuteButtonText(bool muted)
  {
    muteText.text = muted ? "Unmute" : "Mute";
  }

  public void SetNumCollectibles(int numCollectibles, int maxNumCollectibles)
  {
    collectiblesNumText.text = $"{numCollectibles}/{maxNumCollectibles}";
  }

  public IEnumerator ZoomToPlayCo(GameObject virtualCamera)
  {
    yield return ZoomToPositionCo(virtualCamera, cameraGameParameters.cameraOffset, cameraGameParameters.orthographicSize);
  }
  
  public void ZoomToPlay(GameObject virtualCamera)
  {
    ZoomToPosition(virtualCamera, cameraGameParameters.cameraOffset, cameraGameParameters.orthographicSize);
  }

  public IEnumerator ZoomToStart(GameObject virtualCamera)
  {
    yield return ZoomToPositionCo(virtualCamera, cameraStartParameters.cameraOffset, cameraStartParameters.orthographicSize);
  }
  
  public IEnumerator ZoomToStartCo(GameObject virtualCamera)
  {
    yield return ZoomToPositionCo(virtualCamera, cameraStartParameters.cameraOffset, cameraStartParameters.orthographicSize);
  }

  public void ZoomToJump(GameObject virtualCamera)
  {
    ZoomToPosition(virtualCamera, jumpCameraOffset, jumpCameraSize, jumpZoomTime);
  }

  public void InstaZoomToStart(GameObject virtualCamera)
  {
    InstaZoomToPosition(virtualCamera, cameraStartParameters.cameraOffset, cameraStartParameters.orthographicSize);
  }

  private void InstaZoomToPosition(GameObject virtualCamera, Vector2 offset, float orthographicSize)
  {
    virtualCamera.GetComponentInChildren<CinemachineCameraOffset>().m_Offset = offset;
    virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Lens.OrthographicSize = orthographicSize;
  }

  private void ZoomToPosition(GameObject virtualCamera, Vector2 offset, float orthographicSize, float zoomTime = cameraZoomTime)
  {
        /*
    PrimeTween.Tween.Custom(
      (Vector2)virtualCamera.GetComponentInChildren<CinemachineCameraOffset>().m_Offset,
      offset,
      new TweenSettings(zoomTime),
      vec => virtualCamera.GetComponentInChildren<CinemachineCameraOffset>().m_Offset = vec
    );
    
    PrimeTween.Tween.Custom(
      virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Lens.OrthographicSize,
      orthographicSize,
      new TweenSettings(zoomTime),
      x => virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Lens.OrthographicSize = x
    );
        */
  }
  
  private IEnumerator ZoomToPositionCo(GameObject virtualCamera, Vector2 offset, float orthographicSize, float zoomTime = cameraZoomTime)
  {
        /*
    yield return Sequence.Create(Tween.Custom(
      (Vector2)virtualCamera.GetComponentInChildren<CinemachineCameraOffset>().m_Offset,
      offset,
      new TweenSettings(zoomTime),
      vec => virtualCamera.GetComponentInChildren<CinemachineCameraOffset>().m_Offset = vec
    )).Group(Tween.Custom(
      virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Lens.OrthographicSize,
      orthographicSize,
      new TweenSettings(zoomTime),
      x => virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Lens.OrthographicSize = x
    )).ToYieldInstruction();
        */
        yield return null;
  }
}