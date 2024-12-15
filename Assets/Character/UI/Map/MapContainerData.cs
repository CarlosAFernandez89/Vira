using UnityEngine;
using UnityEngine.Serialization;

namespace Character.UI.Map
{
    public class MapContainerData : MonoBehaviour
    {
        public SceneField roomScene;

        public bool HasBeenRevealed { get; set; }
        
    }
}
