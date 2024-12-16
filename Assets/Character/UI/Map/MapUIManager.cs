using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

namespace Character.UI.Map
{
    public class MapUIManager : MonoBehaviour
    {
        public RenderTexture mapRenderTexture; // Assign this in the Inspector
        public InputActionAsset inputActions; // Assign this in the Inspector
        public UnityEngine.Camera mapCamera; // The camera used to render the map (optional, for zoom)

        private InputAction zoomInAction;
        private InputAction zoomOutAction;
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
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 15f;
        [SerializeField] private float zoomSpeed = 2f;  // Speed of zoom (adjust as needed)
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
        
        private void Awake()
        {
            cameraZPosition = mapCamera.transform.position.z;
            targetPosition = new Vector3(mapCamera.transform.position.x, mapCamera.transform.position.y, cameraZPosition);
            
            // Get input actions
            zoomInAction = inputActions.FindActionMap("UI").FindAction("ZoomIn"); // Add ZoomIn and ZoomOut actions to your Input Actions
            zoomOutAction = inputActions.FindActionMap("UI").FindAction("ZoomOut");
            moveCameraAction = inputActions.FindActionMap("UI").FindAction("Navigate"); // New action for camera movement
            dragStartAction  = inputActions.FindActionMap("UI").FindAction("DragCameraStart");
            dragDeltaAction = inputActions.FindActionMap("UI").FindAction("DragCameraDelta");

            // Bind actions with repetition handling
            zoomInAction.performed += ctx => { isZoomInHeld = true; timer = 0; Zoom(1); }; // Immediate zoom on press
            zoomInAction.canceled += ctx => isZoomInHeld = false;

            zoomOutAction.performed += ctx => { isZoomOutHeld = true; timer = 0; Zoom(-1); }; // Immediate zoom on press
            zoomOutAction.canceled += ctx => isZoomOutHeld = false;
            
            moveCameraAction.performed += ctx =>
            {
                // Determine the target position based on input
                targetPosition += new Vector3(ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y, cameraZPosition) * moveSpeed;
                isMoving = true;
            };
            
            moveCameraAction.canceled += ctx =>
            {
                isMoving = false;
            };
            
            // Drag camera action
            dragStartAction.performed += ctx =>
            {
                dragStartMousePosition = Mouse.current.position.ReadValue();
                isDragging = true;
            };

            dragStartAction.canceled += ctx =>
            {
                isDragging = false;
            };
            
            resetCameraAction = inputActions.FindActionMap("UI").FindAction("ResetCamera");
            resetCameraAction.performed += ctx => ResetCamera();
        }
        
        public void InitializeMap(VisualElement root)
        {
            // Get reference to the Image element from the UXML
            mapImage = root.Q<Image>("map-image");
            mapImage.image = mapRenderTexture;
            
            mapCamera.orthographicSize = currentZoom;
            targetZoom = currentZoom;
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
            
            // Smoothly interpolate the orthographic size towards the target zoom
            if (mapCamera != null && mapCamera.orthographic)
            {
                if (Mathf.Abs(mapCamera.orthographicSize - targetZoom) > 0.01f)
                {
                    mapCamera.orthographicSize = 
                        Mathf.Lerp(mapCamera.orthographicSize, targetZoom, zoomLerpSpeed * Time.unscaledDeltaTime);
                }
            }
            
            // Only move the camera if isMoving is true or if we are still smoothing towards the target
            if (isMoving || Vector2.Distance(mapCamera.transform.position, targetPosition) > 0.01f)
            {
               //MoveCamera();
            }
        }
        
        private void Zoom(float direction)
        {
            targetZoom = Mathf.Clamp(currentZoom + direction * zoomSpeed * repeatRate, minZoom, maxZoom);
            currentZoom = targetZoom;
        }
        
        private void MoveCamera()
        {
            if (mapCamera != null)
            {
                // Smoothly move the camera towards the target position
                mapCamera.transform.position = 
                    Vector2.Lerp(mapCamera.transform.position, targetPosition, zoomLerpSpeed * Time.unscaledDeltaTime);
            }
        }
        
        private void DragCamera()
        {
            if (mapCamera != null)
            {
                // Read the delta from the DragDelta action
                Vector2 mouseDelta = dragDeltaAction.ReadValue<Vector2>();

                // Only move the camera if the mouse has actually moved (delta is not zero)
                if (mouseDelta != Vector2.zero)
                {
                    Vector3 move = new Vector3(-mouseDelta.x, -mouseDelta.y, 0) * mouseDragSpeed * Time.unscaledDeltaTime;
                    mapCamera.transform.position += move;
                }
            }
        }
        
        private void ResetCamera()
        {
            // Set the target position to Vector3.zero (or your desired reset position)
            targetPosition = new Vector3(owningCharacter.transform.position.x,owningCharacter.transform.position.y, cameraZPosition);

            // Reset isMoving to false, as we are manually controlling the movement
            isMoving = false;

            // You can optionally reset other camera properties here, like zoom:
            // targetZoom = initialZoom; 

            Debug.Log("Camera reset initiated.");
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