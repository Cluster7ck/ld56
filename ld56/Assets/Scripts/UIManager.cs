using UnityEngine;
using DG.Tweening;
public class CameraSceneParameters
{
    public CameraSceneParameters(float pOrthographicSize, float pXcameraOffset, float pYcameraOffset) {
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

    [SerializeField] private float cameraZoomTime = 2f;

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

    public void ZoomToPlay(GameObject virtualCamera) {
        ZoomToPosition(virtualCamera, cameraGameParameters.cameraOffset, cameraGameParameters.orthographicSize);
    }

    public void ZoomToStart(GameObject virtualCamera) {
        ZoomToPosition(virtualCamera, cameraStartParameters.cameraOffset, cameraStartParameters.orthographicSize);
    }

    private void ZoomToPosition(GameObject virtualCamera, Vector2 offset, float orthographicSize) {
        DOTween.To(() => (Vector2)virtualCamera.GetComponentInChildren<CinemachineCameraOffset>().m_Offset, xy => virtualCamera.GetComponentInChildren<CinemachineCameraOffset>().m_Offset = xy, offset, cameraZoomTime);
        DOTween.To(() => virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Lens.OrthographicSize, x => virtualCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().m_Lens.OrthographicSize = x, orthographicSize, cameraZoomTime);
    }

}