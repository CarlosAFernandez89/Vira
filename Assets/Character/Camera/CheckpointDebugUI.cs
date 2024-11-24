using UnityEngine;

namespace Character.Camera
{
    public class CheckpointDebugUI : MonoBehaviour
    {
        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        
            if (GUILayout.Button("Show Checkpoint File Location"))
            {
                SpawnManager.Instance.LogCheckpointPath();
            }
        
            if (GUILayout.Button("Open Checkpoint Folder"))
            {
                SpawnManager.Instance.OpenCheckpointFolder();
            }
        
            if (GUILayout.Button("Delete Checkpoint File"))
            {
                SpawnManager.Instance.DeleteCheckpointFile();
            }
        
            GUILayout.EndArea();
#endif
        }
    }
}
