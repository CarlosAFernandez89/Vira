using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Character
{
    public class SpawnManager : MonoBehaviour
    {
        // Singleton instance
        private static SpawnManager _instance;
        public static SpawnManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<SpawnManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("SpawnManager");
                        _instance = go.AddComponent<SpawnManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Spawn Settings")]
        [SerializeField] private Transform defaultSpawnPoint;
        [SerializeField] private GameObject playerPrefab;

        [Header("Effects")]
        [SerializeField] private GameObject spawnVFXPrefab;
        [SerializeField] private GameObject checkpointVFXPrefab;
        [SerializeField] private float spawnAnimationDuration = 1f;
        [SerializeField] private AnimationCurve spawnScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private bool useSpawnAnimation = true;

        [Header("Checkpoint Settings")]
        [SerializeField] private bool persistCheckpoints = true;
        [SerializeField] private string saveFileName = "checkpoints.dat";

        // Internal variables
        private Transform currentSpawnPoint;
        private GameObject currentPlayer;
        private Dictionary<string, CheckpointData> checkpoints = new Dictionary<string, CheckpointData>();

        [System.Serializable]
        private class CheckpointData
        {
            public Vector3 position;
            public Quaternion rotation;
            public DateTime timestamp;
            public Dictionary<string, object> customData;

            public CheckpointData(Vector3 pos, Quaternion rot)
            {
                position = pos;
                rotation = rot;
                timestamp = DateTime.Now;
                customData = new Dictionary<string, object>();
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            Initialize();
        
            // Only make the manager persistent after initialization
            if (transform.parent == null) // Check if it's not a child object
            {
                //DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            Spawn();
        }

        private void Initialize()
        {
            // Set default spawn point if none is assigned
            if (defaultSpawnPoint == null)
            {
                Debug.LogWarning("No default spawn point assigned. Creating one at origin.");
                GameObject spawnPoint = new GameObject("DefaultSpawnPoint");
                defaultSpawnPoint = spawnPoint.transform;
            }

            currentSpawnPoint = defaultSpawnPoint;
            LoadCheckpoints();
        }

        public GameObject Spawn()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("No player prefab assigned to SpawnManager!");
                return null;
            }

            Vector3 spawnPosition = currentSpawnPoint.position;
            Quaternion spawnRotation = currentSpawnPoint.rotation;

            // Spawn VFX
            if (spawnVFXPrefab != null)
            {
                Instantiate(spawnVFXPrefab, spawnPosition, Quaternion.identity);
            }

            // Spawn player
            currentPlayer = Instantiate(playerPrefab, spawnPosition, spawnRotation);

            // Apply spawn animation
            if (useSpawnAnimation)
            {
                StartCoroutine(PlaySpawnAnimation(currentPlayer));
            }

            return currentPlayer;
        }

        private IEnumerator PlaySpawnAnimation(GameObject player)
        {
            if (player == null) yield break;

            // Store original scale
            Vector3 originalScale = player.transform.localScale;
        
            // Temporarily disable player input/physics during animation
            Rigidbody rb = player.GetComponent<Rigidbody>();
            bool wasKinematic = false;
            if (rb != null)
            {
                wasKinematic = rb.isKinematic;
                rb.isKinematic = true;
            }

            // Animation loop
            float elapsed = 0f;
            while (elapsed < spawnAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / spawnAnimationDuration;
                float scale = spawnScaleCurve.Evaluate(progress);
                player.transform.localScale = originalScale * scale;
                yield return null;
            }

            // Restore original scale and physics
            player.transform.localScale = originalScale;
            if (rb != null)
            {
                rb.isKinematic = wasKinematic;
            }
        }

        public void Respawn()
        {
            if (currentPlayer != null)
            {
                StartCoroutine(RespawnSequence());
            }
            else
            {
                Spawn();
            }
        }

        private IEnumerator RespawnSequence()
        {
            // Play death/despawn animation if needed
            if (currentPlayer != null)
            {
                // Optional: Play death animation
                yield return new WaitForSeconds(0.5f); // Adjust timing as needed
                Destroy(currentPlayer);
            }

            yield return new WaitForSeconds(0.2f); // Brief pause before respawn
            Spawn();
        }

        public void TeleportToLastSpawnPoint()
        {
            if (currentPlayer == null)
            {
                Spawn();
                return;
            }

            StartCoroutine(TeleportSequence());
        }

        private IEnumerator TeleportSequence()
        {
            // Optional: Play teleport start VFX
            if (spawnVFXPrefab != null)
            {
                Instantiate(spawnVFXPrefab, currentPlayer.transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.1f);

            currentPlayer.transform.position = currentSpawnPoint.position;
            currentPlayer.transform.rotation = currentSpawnPoint.rotation;

            // Optional: Play teleport end VFX
            if (spawnVFXPrefab != null)
            {
                Instantiate(spawnVFXPrefab, currentPlayer.transform.position, Quaternion.identity);
            }
        }

        public void RegisterCheckpoint(string checkpointId, Vector3 position, Quaternion rotation, Dictionary<string, object> customData = null)
        {
            CheckpointData checkpoint = new CheckpointData(position, rotation);
        
            if (customData != null)
            {
                checkpoint.customData = new Dictionary<string, object>(customData);
            }

            checkpoints[checkpointId] = checkpoint;
            UpdateCurrentSpawnPoint(position, rotation);

            // Play checkpoint VFX
            if (checkpointVFXPrefab != null)
            {
                Instantiate(checkpointVFXPrefab, position, Quaternion.identity);
            }

            if (persistCheckpoints)
            {
                SaveCheckpoints();
            }
        }

        private void UpdateCurrentSpawnPoint(Vector3 position, Quaternion rotation)
        {
            if (currentSpawnPoint == defaultSpawnPoint)
            {
                GameObject newSpawnPoint = new GameObject("CurrentSpawnPoint");
                currentSpawnPoint = newSpawnPoint.transform;
            }

            currentSpawnPoint.position = position;
            currentSpawnPoint.rotation = rotation;
        }

        private void SaveCheckpoints()
        {
            try
            {
                string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
                using (FileStream stream = new FileStream(savePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, checkpoints);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save checkpoints: {e.Message}");
            }
        }

        private void LoadCheckpoints()
        {
            try
            {
                string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
                if (File.Exists(savePath))
                {
                    using (FileStream stream = new FileStream(savePath, FileMode.Open))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        checkpoints = (Dictionary<string, CheckpointData>)formatter.Deserialize(stream);

                        // Set current spawn point to last checkpoint
                        if (checkpoints.Count > 0)
                        {
                            // Find the checkpoint with the latest timestamp
                            CheckpointData lastCheckpoint = null;
                            DateTime latestTime = DateTime.MinValue;

                            foreach (var checkpoint in checkpoints.Values)
                            {
                                if (checkpoint.timestamp > latestTime)
                                {
                                    latestTime = checkpoint.timestamp;
                                    lastCheckpoint = checkpoint;
                                }
                            }

                            if (lastCheckpoint != null)
                            {
                                UpdateCurrentSpawnPoint(lastCheckpoint.position, lastCheckpoint.rotation);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load checkpoints: {e.Message}");
                checkpoints = new Dictionary<string, CheckpointData>();
            }
        }

        public void ResetCheckpoints()
        {
            checkpoints.Clear();
            currentSpawnPoint = defaultSpawnPoint;
        
            if (persistCheckpoints)
            {
                string savePath = Path.Combine(Application.persistentDataPath, saveFileName);
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }
            }
        }

        public Dictionary<string, object> GetCheckpointCustomData(string checkpointId)
        {
            if (checkpoints.TryGetValue(checkpointId, out CheckpointData checkpoint))
            {
                return new Dictionary<string, object>(checkpoint.customData);
            }
            return null;
        }
        
        public string GetCheckpointFilePath()
        {
            return Path.Combine(Application.persistentDataPath, saveFileName);
        }

        public void LogCheckpointPath()
        {
            Debug.Log($"Checkpoint file location: {GetCheckpointFilePath()}");
        }

        // Optional: Add function to open the folder in file explorer
        public void OpenCheckpointFolder()
        {
            string folderPath = Application.persistentDataPath;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            System.Diagnostics.Process.Start("explorer.exe", folderPath);
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            System.Diagnostics.Process.Start("open", folderPath);
#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
            System.Diagnostics.Process.Start("xdg-open", folderPath);
#endif
        }

        // Optional: Add function to delete save file
        public void DeleteCheckpointFile()
        {
            string filePath = GetCheckpointFilePath();
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"Deleted checkpoint file at: {filePath}");
            }
            else
            {
                Debug.Log("No checkpoint file found to delete.");
            }
        }
    }
}