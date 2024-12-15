using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utilities
{
    public class EnableAtRuntimeManager : MonoBehaviour
    {
        private static EnableAtRuntimeManager _instance;

        public static EnableAtRuntimeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var managerObject = new GameObject("EnableAtRuntimeManager");
                    _instance = managerObject.AddComponent<EnableAtRuntimeManager>();
                    DontDestroyOnLoad(managerObject);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HandleEnableAtRuntimeObjects();
        }

        private void HandleEnableAtRuntimeObjects()
        {
            EnableAtRuntime[] objectsToManage = FindObjectsByType<EnableAtRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (var obj in objectsToManage)
            {
                switch (obj.behavior)
                {
                    case EnableAtRuntime.EnableBehavior.AlwaysEnable:
                        obj.gameObject.SetActive(true);
                        break;

                    case EnableAtRuntime.EnableBehavior.EnableInActiveScene:
                        obj.gameObject.SetActive(SceneManager.GetActiveScene() == obj.gameObject.scene);
                        break;

                    case EnableAtRuntime.EnableBehavior.DisableWhenSceneInactive:
                        obj.gameObject.SetActive(SceneManager.GetActiveScene() != obj.gameObject.scene);
                        break;

                    default:
                        Debug.LogWarning($"Unhandled EnableBehavior: {obj.behavior}");
                        break;
                }
            }
        }
    }
}
