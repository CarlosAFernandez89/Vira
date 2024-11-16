using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character
{
    public class CameraManager : MonoBehaviour
    {
        public static CameraManager instance;
        
        [SerializeField] private CinemachineCamera[] allCinemachineCameras;
        
        [Header("Controls for lerp the Y Damping during player jump/fall.")] 
        [SerializeField] private float fallPanAmount = 0.25f;
        [SerializeField] private float fallYPanTime = 0.35f;
        public float fallSpeedYDampingChangeThreshold = -15f;
        
        public bool IsLerpingYDamping { get; private set; }
        public bool LerpedFromPlayerFalling { get; set; }
        
        private Coroutine _lerpYPanCoroutine;
        private Coroutine _panCameraCoroutine;

        private CinemachineCamera _currentCamera;
        private CinemachinePositionComposer _positionComposer;
        
        private float _normYPanAmount;

        private Vector2 _startingTrackedObjectOffset;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }

            foreach (var cinemachineCamera in allCinemachineCameras)
            {
                if (cinemachineCamera.enabled)
                {
                    _currentCamera = cinemachineCamera;
                    _positionComposer = _currentCamera.GetComponent<CinemachinePositionComposer>();
                }
            }

            _normYPanAmount = _positionComposer.Damping.y;

            _startingTrackedObjectOffset = _positionComposer.TargetOffset;
        }

        public void LerpYDamping(bool isPlayerFalling)
        {
            _lerpYPanCoroutine = StartCoroutine(LerpYAction(isPlayerFalling));
        }

        private IEnumerator LerpYAction(bool isPlayerFalling)
        {
            IsLerpingYDamping = true;
            
            float startDampAmount = _positionComposer.Damping.y;
            float endDampAmount = 0f;

            if (isPlayerFalling)
            {
                endDampAmount = fallPanAmount;
                LerpedFromPlayerFalling = true;
            }
            else
            {
                endDampAmount = _normYPanAmount;
            }
            float elapsedTime = 0f;
            while (elapsedTime < fallYPanTime)
            {
                elapsedTime += Time.deltaTime;
                
                float lerpedPanAmount = Mathf.Lerp(startDampAmount, endDampAmount, elapsedTime / fallYPanTime);
                _positionComposer.Damping.y = lerpedPanAmount;
                yield return null;
            }
            IsLerpingYDamping = false;
        }

        #region Pan Camera

        public void PanCameraOnContact(float panDistance, float panTime, PanDirection panDirection,
            bool panToStartingPosition)
        {
            _panCameraCoroutine = StartCoroutine(PanCamera(panDistance, panTime, panDirection, panToStartingPosition));
        }

        private IEnumerator PanCamera(float panDistance, float panTime, PanDirection panDirection,
            bool panToStartingPosition)
        {
            Vector2 endPosition = Vector2.zero;
            Vector2 startPosition = Vector2.zero;

            if (!panToStartingPosition)
            {
                switch (panDirection)
                {
                    case PanDirection.Up:
                        endPosition = Vector2.up;
                        break;
                    case PanDirection.Down:
                        endPosition = Vector2.down;
                        break;
                    case PanDirection.Left:
                        endPosition = Vector2.left;
                        break;
                    case PanDirection.Right:
                        endPosition = Vector2.right;
                        break;
                    default:
                        break;
                }
                
                endPosition *= panDistance;
                startPosition = _startingTrackedObjectOffset;
                endPosition += startPosition;
            }
            else
            {
                startPosition = _positionComposer.TargetOffset;
                endPosition = _startingTrackedObjectOffset;
            }
            
            float elapsedTime = 0f;
            while (elapsedTime < panTime)
            {
                elapsedTime += Time.deltaTime;
                Vector2 panLerp = Vector2.Lerp(startPosition, endPosition, elapsedTime / panTime);
                _positionComposer.TargetOffset = panLerp;
                
                yield return null;
            }
        }

        #endregion

        #region Swap Cameras

        public void SwapCamera(CinemachineCamera leftCamera, CinemachineCamera rightCamera,
            Vector2 triggerExitDirection)
        {
            if (_currentCamera == leftCamera && triggerExitDirection.x > 0f)
            {
                rightCamera.enabled = true;
                
                leftCamera.enabled = false;
                
                _currentCamera = rightCamera;
                
                _positionComposer = _currentCamera.GetComponent<CinemachinePositionComposer>();
            }
            else if (_currentCamera == rightCamera && triggerExitDirection.x < 0f)
            {
                leftCamera.enabled = true;
                rightCamera.enabled = false;
                _currentCamera = leftCamera;
                _positionComposer = _currentCamera.GetComponent<CinemachinePositionComposer>();
            }
        }

        #endregion
    }
}
