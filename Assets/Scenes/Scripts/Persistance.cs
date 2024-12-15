using UnityEngine;

namespace Scenes.Scripts
{
    public class Persistance : MonoBehaviour
    {
        public static Persistance Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
