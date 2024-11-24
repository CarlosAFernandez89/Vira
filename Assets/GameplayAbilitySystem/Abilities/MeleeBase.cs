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
        private GameObject _meleeInstance;

        private void OnEnable()
        {
        }

        protected override void StartAbility(GameObject user)
        {
            base.StartAbility(user);

            if (startVFXPrefab != null)
            {
                GameObject vfxInstance = Instantiate(startVFXPrefab, user.transform.position, Quaternion.identity);
                Destroy(vfxInstance, 2.0f);
            }
        }
        
        protected override void HandleAnimationEvent(string eventName)
        {
            switch (eventName)
            {
                case "SpawnCollider":
                    SpawnMeleeCollision();
                    break;
                case "DestroyCollider":
                    DestroyMeleeCollision();
                    break;
                case "EndAbility":
                    EndAbility();
                    break;
                // Add more cases as needed
            }
        }
        
        private void SpawnMeleeCollision()
        {
            if (meleeCollisionPrefab != null && currentUser != null)
            {
                Vector3 spawnPosition = currentUser.transform.position +
                                        new Vector3(currentUser.transform.right.x * collisionSpawnOffset.x, 
                                            collisionSpawnOffset.y, 0);

                _meleeInstance = Instantiate(meleeCollisionPrefab, spawnPosition, Quaternion.identity);

                if (_meleeInstance != null)
                {
                    _capsuleCollider = _meleeInstance.GetComponent<CapsuleCollider2D>();
                    CheckCollisions();
                }
            }
        }

        private void DestroyMeleeCollision()
        {
            if (_meleeInstance != null)
            {
                Destroy(_meleeInstance);
                _meleeInstance = null;
            }
        }
        
        private void CheckCollisions()
        {
            if (_capsuleCollider == null) return;

            ContactFilter2D contactFilter = new ContactFilter2D
            {
                layerMask = LayerMask.GetMask("AbilityCollision"),
                useLayerMask = true
            };

            Collider2D[] collider2Ds = new Collider2D[maxTargetCount];
            int overlapCount = _capsuleCollider.Overlap(contactFilter, collider2Ds);

            if (overlapCount > 0)
            {
                foreach (var collider in collider2Ds)
                {
                    if (collider == null) continue;
                    
                    AbilitySystemComponent ASC = collider.gameObject.GetComponent<AbilitySystemComponent>();
                    if (ASC != null)
                    {
                        ASC.ApplyEffect(gameplayEffect);
                    }
                }
            }
        }

        protected override void ActivateAbility(GameObject user)
        {
            base.ActivateAbility(user);
            
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
            DestroyMeleeCollision();
            base.EndAbility();
        }

        protected override void ApplyCost(GameObject user)
        {
            base.ApplyCost(user);
        }
    }
}
