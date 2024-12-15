using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Character.UI.Map
{
    public class MapRoomManager : MonoBehaviour
    {
        public static MapRoomManager Instance;

        private MapContainerData[] _rooms;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            
            _rooms = GetComponentsInChildren<MapContainerData>(true);
        }

        public void RevealNewRoom()
        {
            string newLoadedScene = SceneManager.GetActiveScene().name;

            foreach (var room in _rooms)
            {
                if (room.roomScene.SceneName != newLoadedScene || room.HasBeenRevealed) continue;
                
                room.gameObject.SetActive(true);
                room.HasBeenRevealed = true;
                return;
            }
        }
    }
}
