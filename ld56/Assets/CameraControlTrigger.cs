using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Cinemachine;


public class CameraControlTrigger : MonoBehaviour
{
    public CustomInspectorObjects customInspectorObjects;

    private Collider2D _coll;

    private void Start () {
        _coll = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D ( Collider2D collision ) {
        if(collision.CompareTag("Player")) {
            if(customInspectorObjects.panCameraOnContact) {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panValue, customInspectorObjects.panTime, false);
            }
            if(customInspectorObjects.zoomCameraOnContact) {
                CameraManager.instance.ZoomCameraOnContact(customInspectorObjects.orthographicSize, customInspectorObjects.zoomTime, false);
            }
        }
    }

    private void OnTriggerExit2D ( Collider2D collision ) {
        if(collision.CompareTag("Player")) {
            if(customInspectorObjects.panCameraOnContact) {
                CameraManager.instance.PanCameraOnContact(customInspectorObjects.panValue, customInspectorObjects.panTime, true);
            }
            if(customInspectorObjects.zoomCameraOnContact) {
                CameraManager.instance.ZoomCameraOnContact(customInspectorObjects.orthographicSize, customInspectorObjects.zoomTime, true);
            }
        }
    }
}

[System.Serializable]
public class CustomInspectorObjects
{
    public bool swapCameras = false;
    [HideInInspector] public CinemachineVirtualCamera cameraOnLeft;
    [HideInInspector] public CinemachineVirtualCamera cameraOnRight;
    
    public bool panCameraOnContact = false;
    [HideInInspector] public Vector2 panValue;
    [HideInInspector] public float panTime = 0.35f;
    
    public bool zoomCameraOnContact = false;
    [HideInInspector] public float orthographicSize = 5f;
    [HideInInspector] public float zoomTime = 2f;
}

[CustomEditor(typeof(CameraControlTrigger))]
public class MyScriptEditor : Editor {
    CameraControlTrigger cameraControlTrigger;

    private void OnEnable () {
        cameraControlTrigger = (CameraControlTrigger)target;
    }

    public override void OnInspectorGUI () {
        DrawDefaultInspector();

        if (cameraControlTrigger.customInspectorObjects.swapCameras) {
            cameraControlTrigger.customInspectorObjects.cameraOnLeft = EditorGUILayout.ObjectField("Camera on Left", cameraControlTrigger.customInspectorObjects.cameraOnLeft, 
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;

            cameraControlTrigger.customInspectorObjects.cameraOnRight = EditorGUILayout.ObjectField("Camera on Right", cameraControlTrigger.customInspectorObjects.cameraOnRight, 
                typeof(CinemachineVirtualCamera), true) as CinemachineVirtualCamera;
        }

        if(cameraControlTrigger.customInspectorObjects.panCameraOnContact) {
            cameraControlTrigger.customInspectorObjects.panValue = EditorGUILayout.Vector2Field("Pan Value", cameraControlTrigger.customInspectorObjects.panValue);
            cameraControlTrigger.customInspectorObjects.panTime = EditorGUILayout.FloatField("Pan Time", cameraControlTrigger.customInspectorObjects.panTime);
        }

        if(cameraControlTrigger.customInspectorObjects.zoomCameraOnContact) {
            cameraControlTrigger.customInspectorObjects.orthographicSize = EditorGUILayout.FloatField("Camera Orthographic Size", cameraControlTrigger.customInspectorObjects.orthographicSize);
            cameraControlTrigger.customInspectorObjects.zoomTime = EditorGUILayout.FloatField("Zoom Time", cameraControlTrigger.customInspectorObjects.zoomTime);
        }

        if(GUI.changed) {
            EditorUtility.SetDirty(cameraControlTrigger);
        }
    }
}