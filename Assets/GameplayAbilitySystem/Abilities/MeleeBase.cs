using GameplayAbilitySystem.Attributes;
using GameplayAbilitySystem.GameplayEffects;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameplayAbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "New Melee Ability", menuName = "Abilities/Melee Ability")]
    public class MeleeBase : GameplayAbilityBase
    {
        [Header("Melee Base")]
        [SerializeField] public GameplayEffectBase gameplayEffect;
        [SerializeField] public int maxTargetCount = 1;
        [SerializeField] public GameObject meleeCollisionPrefab;
        [SerializeField] public GameObject startVFXPrefab;
        [SerializeField] public Vector2 collisionSpawnOffset = new Vector2(0f, 0f);
        
        CapsuleCollider2D _capsuleCollider;

        private void OnEnable()
        {
        }

        protected override void StartAbility(GameObject user)
        {
            // Custom VFX logic specific to the Melee Ability
            if (startVFXPrefab != null)
            {
                GameObject vfxInstance = Instantiate(startVFXPrefab, user.transform.position, Quaternion.identity);
                Destroy(vfxInstance, 2.0f);  // Adjust based on the VFX duration
            }
            
            // Proceed to main action
            base.StartAbility(user);  // Calls ActivateAbility(user)
        }

        protected override void ActivateAbility(GameObject user)
        {
            // Main ability action: Spawn a fireball
            if (meleeCollisionPrefab != null)
            {
                GameObject melee = Instantiate(meleeCollisionPrefab, 
                    user.transform.position + 
                    new Vector3(user.transform.right.x + collisionSpawnOffset.x, collisionSpawnOffset.y, 0), Quaternion.identity);
                // Additional logic for fireball behavior (e.g., movement, applying damage)

                if (melee != null)
                {
                    _capsuleCollider = melee.GetComponent<CapsuleCollider2D>();
                    
                    //Filter out everything that is not damageable
                    ContactFilter2D contactFilter = new ContactFilter2D();
                    contactFilter.layerMask = LayerMask.GetMask("AbilityCollision");
                    contactFilter.useLayerMask = true;
                    
                    // Init to max target count (default : 1)
                    Collider2D[] collider2Ds = new Collider2D[maxTargetCount];
                    
                    int overlapCount = _capsuleCollider.Overlap(contactFilter, collider2Ds);
                    if (overlapCount > 0)
                    {
                        foreach (var collider in collider2Ds)
                        {
                            AbilitySystemComponent ASC = collider.gameObject.GetComponent<AbilitySystemComponent>();
                            if(ASC != null)
                            {
                                ASC.ApplyEffect(gameplayEffect);
                            }
                        }
                    }
                }
                Destroy(melee);
            }
        }
        

        protected override void EndAbility()
        {
            base.EndAbility();
        }

        protected override void ApplyCost(GameObject user)
        {
            base.ApplyCost(user);
        }
    }
}
