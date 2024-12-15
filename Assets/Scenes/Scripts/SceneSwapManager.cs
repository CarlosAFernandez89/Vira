using System.Collections;
using System.Collections.Generic;
using Character.UI.Map;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using WorldObjects.Door;

namespace Scenes.Scripts
{
    public class SceneSwapManager : MonoBehaviour
    {
        public static SceneSwapManager Instance;

        private static bool _loadFromDoor;
        
        private DoorBase.DoorToSpawnAt _doorToSpawnAt;
        
        private GameObject _playerObject;
        private CapsuleCollider2D _playerCollider;
        private Collider2D _doorCollider;
        private Vector3 _playerSpawnPosition;
        
        private static SceneField _sceneToLoad;

        [SerializeField] private LayerMask groundLayer;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void SetReferences()
        {
            _playerObject = GameObject.FindGameObjectWithTag("Player");
            if (_playerObject != null)
            {
                _playerCollider = _playerObject.GetComponent<CapsuleCollider2D>();
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetReferences();
            
            if (_loadFromDoor)
            {
                SetPlayerLocation();
            }
            
            MapRoomManager.Instance.RevealNewRoom();
        }

        private void SetPlayerLocation()
        {
            SceneFadeManager.Instance.FadeIn();
            FindDoor(_doorToSpawnAt);
            _playerObject.transform.position = _playerSpawnPosition;
            _loadFromDoor = false;
        }

        public static void SwapSceneFromDoorUse(SceneField myScene, DoorBase.DoorToSpawnAt doorToSpawnAt)
        {
            _loadFromDoor = true;
            Instance.StartCoroutine(Instance.FadeOutThenChangeScene(myScene, doorToSpawnAt));
        }

        private IEnumerator FadeOutThenChangeScene(SceneField myScene,
            DoorBase.DoorToSpawnAt doorToSpawnAt = DoorBase.DoorToSpawnAt.None)
        {
            
            SceneFadeManager.Instance.FadeOut();

            while (SceneFadeManager.Instance.IsFadingOut)
            {
                yield return null;
            }

            _sceneToLoad = myScene;
            _doorToSpawnAt = doorToSpawnAt;

            if (!IsSceneLoaded(myScene))
            {
                SceneManager.LoadSceneAsync(myScene, LoadSceneMode.Additive);
            }
            else
            {
                Debug.Log($"{myScene} is already loaded.");
                SetPlayerLocation();
            }
        }
        
        private bool IsSceneLoaded(string sceneNameOrPath)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == sceneNameOrPath || scene.path == sceneNameOrPath)
                {
                    return scene.isLoaded;
                }
            }
            return false;
        }

        private void FindDoor(DoorBase.DoorToSpawnAt doorSpawnNumber)
        {
            // Iterate through all loaded scenes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                // Check if this is the target scene
                if (scene.name == _sceneToLoad)
                {
                    // Find all DoorBase objects in the target scene
                    DoorBase[] doors = scene.FindObjectsOfType<DoorBase>(); 

                    foreach (var door in doors)
                    {
                        if (door.currentDoorPosition == doorSpawnNumber)
                        {
                            _doorCollider = door.gameObject.GetComponent<Collider2D>();
                            CalculateSpawnPosition();
                            return; // Door found, exit the function
                        }
                    }
                }
            }

            // If the door was not found
            Debug.LogWarning($"Door {doorSpawnNumber} not found in scene {_sceneToLoad}.");
        }

        private void CalculateSpawnPosition()
        {
            // Get the player's collider bounds
            float colliderHeight = _playerCollider.bounds.size.y;
            float colliderHalfHeight = colliderHeight / 2f;

            // Starting position for the raycast (above the door)
            Vector2 raycastStart = _doorCollider.transform.position + new Vector3(0, colliderHeight, 0);

            // Raycast downwards to find the ground, only checking against the ground layer
            RaycastHit2D hit = Physics2D.Raycast(raycastStart, Vector2.down, Mathf.Infinity, groundLayer);

            if (hit.collider != null)
            {
                // Calculate the spawn position:
                // - hit.point: The point where the raycast hit the ground.
                // - Vector2.up * colliderHalfHeight: Offset upwards by half the player's height to prevent clipping into the ground.
                _playerSpawnPosition = hit.point + Vector2.up * colliderHalfHeight;
            }
            else
            {
                // Fallback: If no ground is hit, use the original logic (though this might indicate a problem with the level design)
                Debug.LogWarning("CalculateSpawnPosition: No ground found beneath the door. Using default spawn logic.");
                _playerSpawnPosition = _doorCollider.transform.position - new Vector3(0, colliderHalfHeight, 0);
            }

            // Optional: Visualize the raycast in the editor
            Debug.DrawRay(raycastStart, Vector2.down * (hit.collider != null ? hit.distance : 10f), Color.red, 5f);
        
        }
        
    }
    
    // Extension method to find all objects of a specific type within a scene
    public static class SceneExtensions
    {
        public static T[] FindObjectsOfType<T>(this Scene scene) where T : Component
        {
            List<T> results = new List<T>();
            GameObject[] rootGameObjects = scene.GetRootGameObjects();
            foreach (GameObject rootGameObject in rootGameObjects)
            {
                results.AddRange(rootGameObject.GetComponentsInChildren<T>(true));
            }
            return results.ToArray();
        }
    }
}
