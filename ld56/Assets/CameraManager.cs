using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private CinemachineVirtualCamera[] _allVirtualCameras;

    [SerializeField] private float _fallPanAmount = 0.25f;
    [SerializeField] private float _fallYPanTime = 0.35f;
    public float fallSpeedYDampingChangeThreshold = -15f;


    public bool IsLerpingYDamping { get;  set; }
    public bool LerpedFromPlayerFalling { get;  set; }

    private Coroutine _lerpYPanCoroutine;
    private Coroutine _panCameraCoroutine;
    private Coroutine _zoomCameraCoroutine;

    private CinemachineVirtualCamera _currentCamera;

    private CinemachineFramingTransposer _framingTransposer;

    private float _normYPanAmount = 0f;

    private Vector2 _startingTrackedObjectOffset;

    private float _startingCameraOrthographicSize;

    private void Awake () {
        if(instance == null) {
            instance = this;
        }

        for(int i = 0; i < _allVirtualCameras.Length; i++) {
            if(_allVirtualCameras[i].enabled) {
                // set current active camera
                _currentCamera = _allVirtualCameras[i];

                // set the framing transposer
                _framingTransposer = _currentCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            }
        }

        _normYPanAmount = _framingTransposer.m_YDamping;

        _startingTrackedObjectOffset = _framingTransposer.m_TrackedObjectOffset;
        _startingCameraOrthographicSize = _currentCamera.m_Lens.OrthographicSize;
    }

    #region Y-Lerping
    public void LerpYDamping(bool isPlayerFalling) {
        _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
    }

    private IEnumerator LerpYAction(bool isPlayerFalling) {
        IsLerpingYDamping = true;

        float startDampAmount = _framingTransposer.m_YDamping;
        float endDampAmount = 0f;

        if(isPlayerFalling) {
            endDampAmount = _fallPanAmount;
            LerpedFromPlayerFalling = true;
        } else {
            endDampAmount = _normYPanAmount;
        }

        float elapsedTime = 0f;
        while(elapsedTime < _fallYPanTime) {
            elapsedTime += Time.deltaTime * Time.timeScale;

            float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, (elapsedTime / _fallYPanTime));
            _framingTransposer.m_YDamping = lerpedPanAmount;

            yield return null;
        }

        IsLerpingYDamping = false;
    }
    #endregion lerping

    #region Panning
    public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos) {
        _panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPos));
    }

    private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection, bool panToStartingPos) {
        
        Vector2 startingPos = Vector2.zero;
        Vector2 endPos = Vector2.zero;

        if(!panToStartingPos) {
            switch (panDirection) {
                case PanDirection.Up:
                    endPos = Vector2.up;
                    break;
                case PanDirection.Down:
                    
                    endPos = Vector2.down;
                    break;
                case PanDirection.Left:
                    
                    endPos = Vector2.left;
                    break;
                case PanDirection.Right:
                    
                    endPos = Vector2.right;
                    break;
                default:
                    endPos = Vector2.zero;
                    break;
            }

            endPos *= panDistance;

            startingPos = _startingTrackedObjectOffset;

            endPos += startingPos;
        } else {
            startingPos = _framingTransposer.m_TrackedObjectOffset;
            endPos = _startingTrackedObjectOffset;
        }

        float elapsedTime = 0f;
        while(elapsedTime < panTime) {
            elapsedTime += Time.deltaTime * Time.timeScale;

            Vector3 panLerp = Vector3.Lerp(startingPos, endPos, (elapsedTime / panTime));
            _framingTransposer.m_TrackedObjectOffset = panLerp;

            yield return null;
        }
    }
    #endregion

    #region Zooming
    public void ZoomCameraOnContact(float orthographicSize, float zoomTime, bool zoomToStart) {
        if(_zoomCameraCoroutine != null) {
            StopCoroutine(_zoomCameraCoroutine);
        }
        _zoomCameraCoroutine = StartCoroutine(ZoomCamera(orthographicSize, zoomTime, zoomToStart));
    }

    private IEnumerator ZoomCamera ( float orthographicSize, float zoomTime, bool zoomToStart ) {

        float startSize;
        float endSize;

        if(!zoomToStart) {
            startSize = _startingCameraOrthographicSize;
            endSize = orthographicSize;
        } else {
            startSize = _currentCamera.m_Lens.OrthographicSize;
            endSize = _startingCameraOrthographicSize;
        }
        
        float elapsedTime = 0f;
        while(elapsedTime < zoomTime) {
            elapsedTime += Time.deltaTime * Time.timeScale;

            float zoomLerp = Mathf.Lerp(startSize, endSize, (elapsedTime / zoomTime));
            _currentCamera.m_Lens.OrthographicSize = zoomLerp;

            yield return null;
        }
    }


    #endregion
    // Start is called before the first frame update
    void Start ()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
