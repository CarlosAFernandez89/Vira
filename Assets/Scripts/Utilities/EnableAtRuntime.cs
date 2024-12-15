using UnityEngine;

namespace Utilities
{
    public class EnableAtRuntime : MonoBehaviour
    {
        public enum EnableBehavior
        {
            AlwaysEnable,            // The object is always enabled when a scene is loaded.
            EnableInActiveScene,     // The object is enabled only when its scene is the active one.
            DisableWhenSceneInactive // The object is disabled if its scene is not the active one.
        }

        [Tooltip("Select how this GameObject should behave when scenes are loaded.")]
        [SerializeField] public EnableBehavior behavior = EnableBehavior.AlwaysEnable;
    }
}
