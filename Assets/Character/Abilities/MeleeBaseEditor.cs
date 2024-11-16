using GameplayAbilitySystem.Abilities;
using UnityEditor;

namespace Character.Abilities
{
    [CustomEditor(typeof(MeleeBase))]
    public class MeleeBaseEditor : AbilityBaseEditor // Inherit from AbilityBaseEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI(); // Call the base class method to draw default inspector
            // Add any additional custom inspector functionality here for MeleeBase
        }
    }
}