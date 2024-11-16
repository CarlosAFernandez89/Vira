using GameplayAbilitySystem;
using UnityEngine;

namespace Character
{
    public class PlayerCharacterBase : MonoBehaviour, IAbilitySystemComponent
    {

        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return GetComponent(typeof(AbilitySystemComponent)) as AbilitySystemComponent;
        }
    }
}
