using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace Character.Camera
{
    public class CameraControlTrigger : MonoBehaviour
    {
        public CustomInspectorObjects CustomInspectorObjects;
        
        private Collider2D _collider2D;

        private bool _transitioned = false;

        private void Start()
        {
            _collider2D = GetComponent<Collider2D>();
            _transitioned = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && Mathf.Approximately(other.GetComponent<Rigidbody2D>().linearVelocity.y, 0f))
            {
                if (CustomInspectorObjects.PanCameraOnContact)
                {
                    CameraManager.instance.PanCameraOnContact(CustomInspectorObjects.PanDistance, CustomInspectorObjects.PanTime, CustomInspectorObjects.PanDirection, false);
                    _transitioned = true;
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!_transitioned)
            {
                if (other.CompareTag("Player") && Mathf.Approximately(other.GetComponent<Rigidbody2D>().linearVelocity.y, 0f))
                {
                    if (CustomInspectorObjects.PanCameraOnContact)
                    {
                        CameraManager.instance.PanCameraOnContact(CustomInspectorObjects.PanDistance, CustomInspectorObjects.PanTime, CustomInspectorObjects.PanDirection, false);
                        _transitioned = true;
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                if (CustomInspectorObjects.SwapCameras && CustomInspectorObjects.CameraOnLeft != null &&
                    CustomInspectorObjects.CameraOnRight != null)
                {
                    Vector2 exitDirection = (other.transform.position - _collider2D.bounds.center).normalized;
                    CameraManager.instance.SwapCamera(CustomInspectorObjects.CameraOnLeft, CustomInspectorObjects.CameraOnRight, exitDirection);
                }
                if (CustomInspectorObjects.PanCameraOnContact)
                {
                    CameraManager.instance.PanCameraOnContact(CustomInspectorObjects.PanDistance, CustomInspectorObjects.PanTime, CustomInspectorObjects.PanDirection, true);
                }
                _transitioned = false;
            }
        }
    }
    
    [System.Serializable]
    public class CustomInspectorObjects
    {
        public bool SwapCameras = false;
        public bool PanCameraOnContact = false;

        [HideInInspector] public CinemachineCamera CameraOnLeft;
        [HideInInspector] public CinemachineCamera CameraOnRight;
        
        [HideInInspector] public PanDirection PanDirection;
        [HideInInspector] public float PanDistance = 3f;
        [HideInInspector] public float PanTime = 0.35f;
    }

    public enum PanDirection
    {
        Up,
        Down,
        Left,
        Right
    }
    
#if UNITY_EDITOR
    
    [CustomEditor(typeof(CameraControlTrigger))]
    public class CameraControlTriggerEditor : Editor
    {
        CameraControlTrigger _cameraControlTrigger;

        private void OnEnable()
        {
            _cameraControlTrigger = (CameraControlTrigger)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (_cameraControlTrigger.CustomInspectorObjects.SwapCameras)
            {
                _cameraControlTrigger.CustomInspectorObjects.CameraOnLeft =
                    EditorGUILayout.ObjectField("Camera On Left",
                        _cameraControlTrigger.CustomInspectorObjects.CameraOnLeft, typeof(CinemachineCamera), 
                        true) 
                        as CinemachineCamera;
                
                _cameraControlTrigger.CustomInspectorObjects.CameraOnRight =
                    EditorGUILayout.ObjectField("Camera On Right",
                        _cameraControlTrigger.CustomInspectorObjects.CameraOnRight,
                        typeof(CinemachineCamera),
                        true) 
                        as CinemachineCamera;
            }

            if (_cameraControlTrigger.CustomInspectorObjects.PanCameraOnContact)
            {
                _cameraControlTrigger.CustomInspectorObjects.PanDirection = (PanDirection)EditorGUILayout.EnumPopup("Camera Pan Direction",
                    _cameraControlTrigger.CustomInspectorObjects.PanDirection);

                _cameraControlTrigger.CustomInspectorObjects.PanDistance = EditorGUILayout.FloatField("Pan Distance",
                    _cameraControlTrigger.CustomInspectorObjects.PanDistance);
                _cameraControlTrigger.CustomInspectorObjects.PanTime = EditorGUILayout.FloatField("Pan Time",
                    _cameraControlTrigger.CustomInspectorObjects.PanTime);
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_cameraControlTrigger);
            }
        }
    }
#endif
}
