using UnityEngine;
using PrimeTween;

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

  [SerializeField] private GameObject startScreen;
  [SerializeField] private GameObject pauseScreen;
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

  private void Start()
  {
    cameraGameParameters.orthographicSize = gameCameraSize;
    cameraGameParameters.cameraOffset = gameCameraOffset;
  }

  public GameObject StartScreen
  {
    get { return startScreen; }
  }

  public GameObject PauseScreen
  {
    get { return pauseScreen; }
  }

  public GameObject EndScreen
  {
    get { return endScreen; }
  }

  public void ZoomToPlay(GameObject virtualCamera)
  {
    ZoomToPosition(virtualCamera, cameraGameParameters.cameraOffset, cameraGameParameters.orthographicSize);
  }

  public void ZoomToStart(GameObject virtualCamera)
  {
    ZoomToPosition(virtualCamera, cameraStartParameters.cameraOffset, cameraStartParameters.orthographicSize);
  }

  public void ZoomToJump(GameObject virtualCamera)
  {
    ZoomToPosition(virtualCamera, jumpCameraOffset, jumpCameraSize, jumpZoomTime);
  }

  private void ZoomToPosition(GameObject virtualCamera, Vector2 offset, float orthographicSize, float zoomTime = cameraZoomTime)
  {
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
  }
}