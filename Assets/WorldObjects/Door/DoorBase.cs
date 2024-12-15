using Character;
using Character.UI.Map;
using Interface;
using Scenes.Scripts;
using UnityEngine;

namespace WorldObjects.Door
{
    public class DoorBase : MonoBehaviour, IInteract
    {
        public enum DoorToSpawnAt
        {
            None,
            One,
            Two,
            Three,
            Four,
        }
        
        [Header("Spawn TO")] 
        [SerializeField] private DoorToSpawnAt doorToSpawnAt;
        [SerializeField] private SceneField sceneToLoad;
        
        [Space(10f)]
        [Header("THIS Door")]
        public DoorToSpawnAt currentDoorPosition;

        public bool LoadingNewScene()
        {
            return sceneToLoad != "";
        }
        public void Interact()
        {
            //Load New Scene
            Debug.Log("DoorBase Interact");

            SceneSwapManager.SwapSceneFromDoorUse(sceneToLoad, doorToSpawnAt);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayerActions>().SetInteractTarget(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                other.gameObject.GetComponent<PlayerActions>().ClearInteractTarget();
            }
        }
    }
}
