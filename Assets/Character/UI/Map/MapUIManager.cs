using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Character.UI.Map
{
    public class MapUIManager : MonoBehaviour, IPlayerSubMenu
    {
        public RenderTexture mapRenderTexture; // Assign this in the Inspector
        public InputActionAsset inputActions; // Assign this in the Inspector
        public UnityEngine.Camera mapCamera; // The camera used to render the map (optional, for zoom)

        private InputAction zoomInAction;
        private InputAction zoomOutAction;
        private InputAction scrollWheelAction;
        private InputAction moveCameraAction;
        private InputAction dragStartAction;
        private InputAction dragDeltaAction;
        private InputAction resetCameraAction;


        private Image mapImage;
        private float currentZoom = 10f;
        private float targetZoom = 1f;
        
        [Header("References")]
        [SerializeField] GameObject owningCharacter;
        
        [Header("Zoom Settings")]
        [SerializeField] private float defaultZoom = 10f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 25f;
        [SerializeField] private float zoomSpeed = 2f;  // Speed of zoom (adjust as needed)
        [FormerlySerializedAs("scrollZoomSpeed")] [SerializeField] private float scrollZoomMultiplier = 5f;
        [SerializeField] private float maxZoomSpeed = 15f;    // Maximum zoom speed
        [SerializeField] private float zoomAcceleration = 5f; // How fast the zoom speed increases (adjust as needed)
        [SerializeField] private float zoomLerpSpeed = 5f; // Speed of the lerp (adjust as needed)
        
        // Variables for input repetition
        private float repeatRate = 0.05f; // Adjust as needed
        private float timer = 0f;
        private bool isZoomInHeld = false;
        private bool isZoomOutHeld = false;
        
        [Header("Camera Movement Settings")]
        [SerializeField] private float moveSpeed = 20f;     // Adjust as needed
        [SerializeField] private float mouseDragSpeed = 5f; // Adjust as needed
        [SerializeField] private float smoothTime = 0.2f;   // Smoothing factor (lower = faster smoothing)
        private Vector2 dragStartMousePosition;
        private bool isDragging = false;
        
        private Vector3 velocity = Vector3.zero;        // For smooth damping
        private Vector3 targetPosition;  // Target position to move towards
        private float cameraZPosition;
        private bool isMoving = false;                  // Flag to indicate if we are moving
        
        
        [Header("Reset Camera Settings")]
        [SerializeField] private float resetSmoothTime = 0.5f; // Smoothing time for reset
        
        
        private bool isMapUIActive = false; // Flag to track if the map UI is active
        
        private void Awake()
        {
            cameraZPosition = mapCamera.transform.position.z;
            targetPosition = new Vector3(mapCamera.transform.position.x, mapCamera.transform.position.y, cameraZPosition);
            
            // Get input actions
            zoomInAction = inputActions.FindActionMap("UI").FindAction("ZoomIn"); // Add ZoomIn and ZoomOut actions to your Input Actions
            zoomOutAction = inputActions.FindActionMap("UI").FindAction("ZoomOut");
            scrollWheelAction = inputActions.FindActionMap("UI").FindAction("ScrollWheel");
            moveCameraAction = inputActions.FindActionMap("UI").FindAction("MoveCamera"); // New action for camera movement
            dragStartAction  = inputActions.FindActionMap("UI").FindAction("DragCameraStart");
            dragDeltaAction = inputActions.FindActionMap("UI").FindAction("DragCameraDelta");
            resetCameraAction = inputActions.FindActionMap("UI").FindAction("ResetCamera");
        }
        
        public void Initialize()
        {
            // Enable the input actions when the UI is active
            isMapUIActive = true;
            EnableInput();
            ResetCamera(); // Move camera to player when UI is opened
        }

        public void Deinitialize()
        {
            // Disable the input actions when the UI is inactive
            isMapUIActive = false;
            DisableInput();
        }
        
        private void BindInput()
        {
            // Bind actions using named methods
            zoomInAction.performed += OnZoomInPerformed;
            zoomInAction.canceled += OnZoomInCanceled;

            zoomOutAction.performed += OnZoomOutPerformed;
            zoomOutAction.canceled += OnZoomOutCanceled;

            scrollWheelAction.performed += OnZoomScrollWheelPerformed;
            scrollWheelAction.canceled += OnZoomScrollWheelCanceled;

            moveCameraAction.performed += OnMoveCameraPerformed;
            moveCameraAction.canceled += OnMoveCameraCanceled;

            dragStartAction.performed += OnDragStartPerformed;
            dragStartAction.canceled += OnDragStartCanceled;

            resetCameraAction.performed += OnResetCameraPerformed;
        }

        private void UnbindInput()
        {
            // Unbind actions using the same named methods
            zoomInAction.performed -= OnZoomInPerformed;
            zoomInAction.canceled -= OnZoomInCanceled;

            zoomOutAction.performed -= OnZoomOutPerformed;
            zoomOutAction.canceled -= OnZoomOutCanceled;
            
            scrollWheelAction.performed -= OnZoomScrollWheelPerformed;
            scrollWheelAction.canceled -= OnZoomScrollWheelCanceled;

            moveCameraAction.performed -= OnMoveCameraPerformed;
            moveCameraAction.canceled -= OnMoveCameraCanceled;

            dragStartAction.performed -= OnDragStartPerformed;
            dragStartAction.canceled -= OnDragStartCanceled;

            resetCameraAction.performed -= OnResetCameraPerformed;
        }
        
        private void EnableInput()
        {
            //inputActions.FindActionMap("UI").Enable();
            BindInput();
        }

        private void DisableInput()
        {
            //inputActions.FindActionMap("UI").Disable();
            UnbindInput();
        }
        
        public void InitializeMap(VisualElement root)
        {
            // Get reference to the Image element from the UXML
            mapImage = root.Q<Image>("map-image");
            mapImage.image = mapRenderTexture;
            
            mapCamera.orthographicSize = defaultZoom;
            targetZoom = defaultZoom;
        }
        
        private void Update()
        {
            // Handle input repetition
            if (isZoomInHeld)
            {
                timer += Time.unscaledDeltaTime;
                if (timer >= repeatRate)
                {
                    zoomSpeed = Mathf.Clamp(zoomSpeed + zoomAcceleration * Time.unscaledDeltaTime, 0, maxZoomSpeed);
                    Zoom(1);
                    timer = 0;
                }
            }

            if (isZoomOutHeld)
            {
                timer += Time.unscaledDeltaTime;
                if (timer >= repeatRate)
                {
                    zoomSpeed = Mathf.Clamp(zoomSpeed + zoomAcceleration * Time.unscaledDeltaTime, 0, maxZoomSpeed);
                    Zoom(-1);
                    timer = 0;
                }
            }
            
            if (isDragging)
            {
                DragCamera();
            }
            
            if (isMoving)
            {
                MoveCamera();
            }
            
            // Smoothly interpolate the orthographic size towards the target zoom
            if (mapCamera != null && mapCamera.orthographic)
            {
                if (Mathf.Abs(mapCamera.orthographicSize - targetZoom) > 0.01f)
                {
                    mapCamera.orthographicSize = 
                        Mathf.Lerp(mapCamera.orthographicSize, targetZoom, zoomLerpSpeed * Time.unscaledDeltaTime);
                }
            }
            
        }
        
        private void OnZoomInPerformed(InputAction.CallbackContext ctx)
        {
            isZoomInHeld = true;
            timer = 0;
            Zoom(1);
        }

        private void OnZoomInCanceled(InputAction.CallbackContext ctx)
        {
            isZoomInHeld = false;
        }

        private void OnZoomOutPerformed(InputAction.CallbackContext ctx)
        {
            isZoomOutHeld = true;
            timer = 0;
            Zoom(-1);
        }

        private void OnZoomOutCanceled(InputAction.CallbackContext ctx)
        {
            isZoomOutHeld = false;
        }

        private void OnZoomScrollWheelPerformed(InputAction.CallbackContext ctx)
        {
            // Read the scroll wheel delta directly from the context
            Vector2 scrollDelta = ctx.ReadValue<Vector2>();

            // Use the Y-axis value to determine the scroll direction
            float zoomDirection = -scrollDelta.y;

            // Apply the zoom direction with speed
            Zoom(zoomDirection * scrollZoomMultiplier);
        }

        private void OnZoomScrollWheelCanceled(InputAction.CallbackContext ctx)
        {
            
        }
        private void Zoom(float direction)
        {
            targetZoom = Mathf.Clamp(currentZoom + direction * zoomSpeed * repeatRate, minZoom, maxZoom);
            currentZoom = targetZoom;
        }
        
        private void OnMoveCameraPerformed(InputAction.CallbackContext ctx)
        {
            if (!isMapUIActive) return;
            
            isMoving = true;
        }

        private void OnMoveCameraCanceled(InputAction.CallbackContext ctx)
        {
            isMoving = false;
        }

        private void OnDragStartPerformed(InputAction.CallbackContext ctx)
        {
            if (!isMapUIActive) return;

            isDragging = true;
        }

        private void OnDragStartCanceled(InputAction.CallbackContext ctx)
        {
            if (!isMapUIActive) return;

            isDragging = false;
        }

        private void OnResetCameraPerformed(InputAction.CallbackContext ctx)
        {
            ResetCamera();
        }
        
        private void MoveCamera()
        {
            if (mapCamera != null && !isDragging)
            {
                Vector2 moveInput = moveCameraAction.ReadValue<Vector2>();
                if (moveInput != Vector2.zero)
                {
                    Vector3 move = new Vector3(moveInput.x, moveInput.y, 0) * moveSpeed * Time.unscaledDeltaTime;
                    targetPosition += move;
                    // Smoothly move the camera towards the target position
                    mapCamera.transform.position =
                        Vector3.Lerp(mapCamera.transform.position, targetPosition, smoothTime);
                }
            }
        }
        
        private void DragCamera()
        {
            if (mapCamera != null && isDragging)
            {
                Vector2 mouseDelta = dragDeltaAction.ReadValue<Vector2>();
                if (mouseDelta != Vector2.zero)
                {
                    Vector3 move = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * mouseDragSpeed * Time.unscaledDeltaTime;
                    targetPosition += move;

                    // Directly update the camera position for responsive dragging
                    mapCamera.transform.position = Vector3.Lerp(mapCamera.transform.position, targetPosition, smoothTime);
                }
            }
        }
        
        private void ResetCamera()
        {
            // Immediately move the camera to the player's position
            if (mapCamera != null)
            {
                targetPosition = new Vector3(owningCharacter.transform.position.x, owningCharacter.transform.position.y, cameraZPosition);
                mapCamera.transform.position = targetPosition;
                
                targetZoom = defaultZoom;
            }
        }
        
        // Call this when you want to take a "snapshot" of the map and update the texture
        public void CaptureMap()
        {
            // If using a canvas, you might capture the canvas here like this
            // canvas.worldCamera = mapCamera;
            // canvas.renderMode = RenderMode.ScreenSpaceCamera;
            // ... capture logic ... 

            // Update the Image element with the new texture
            mapImage.MarkDirtyRepaint(); // Important to tell UI Toolkit to update the image
        }
        
    }
}