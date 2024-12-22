using GameplayAbilitySystem.Attributes;
using GameplayAbilitySystem.GameplayEffects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GameplayAbilitySystem.Abilities
{
    [CreateAssetMenu(fileName = "New Melee Ability", menuName = "Abilities/Melee Ability")]
    public class MeleeBase : GameplayAbilityBase
    {
        [Header("Melee Base")]
        [SerializeField] public int maxTargetCount = 1;
        [SerializeField] protected InputActionReference movementInput;
        [SerializeField] public GameObject meleeCollisionPrefab;
        [SerializeField] public GameObject startVFXPrefab;
        [SerializeField] public Vector2 collisionSpawnOffset = new Vector2(0f, 0f);
        [SerializeField] private GameplayEffectBase onHitEffect;
        
        [Header("Conditional Variables")] 
        [SerializeField] private float groundCheckDistance = 0.25f;
        [SerializeField] private LayerMask groundLayer;
        
        CapsuleCollider2D _capsuleCollider;
        private GameObject _meleeInstance;
        private Rigidbody2D _rigidbody2D;

        public override void OnAbilityGranted(AbilitySystemComponent owningAbilitySystemComponent)
        {
            base.OnAbilityGranted(owningAbilitySystemComponent);
            
            _rigidbody2D = owningAbilitySystemComponent.gameObject.GetComponent<Rigidbody2D>();
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
        
        protected override void ActivateAbility(GameObject user)
        {
            base.ActivateAbility(user);
        }
        
        // Possible animation events to call.
        // Can add more as needed.
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
                case "CheckCollisions":
                    CheckCollisions();
                    break;
                case "EndAbility":
                    EndAbility();
                    break;
                // Add more cases as needed
            }
        }
        
        private void SpawnMeleeCollision()
        {
            if (meleeCollisionPrefab != null && CurrentUser != null)
            {
                Vector2 inputDirection = movementInput.action.ReadValue<Vector2>();
                
                // Determine the spawn direction based on the input
                Vector2 spawnDirection = CurrentUser.transform.right;
                Vector3 spawnPosition = Vector3.zero;
                
                // Up Attack
                if (inputDirection.y > 0.85f)
                {
                    spawnDirection = Vector2.up;
                    
                    spawnPosition = 
                        CurrentUser.transform.position + 
                        new Vector3(0f, spawnDirection.y * collisionSpawnOffset.x);
                    
                } // Down Attack only while not on ground.
                else if (inputDirection.y < -0.85f && !IsGrounded())
                {
                    spawnDirection = Vector2.down;
                    
                    spawnPosition = 
                        CurrentUser.transform.position + 
                        new Vector3(0f, spawnDirection.y * collisionSpawnOffset.x);
                }
                else
                {
                    spawnPosition = 
                        CurrentUser.transform.position + new Vector3(spawnDirection.x * collisionSpawnOffset.x,
                            spawnDirection.y * collisionSpawnOffset.y, 0);
                }

                _meleeInstance = Instantiate(meleeCollisionPrefab, spawnPosition, Quaternion.identity);

                if (_meleeInstance != null)
                {
                    _meleeInstance.transform.SetParent(CurrentUser.transform);
                    _capsuleCollider = _meleeInstance.GetComponent<CapsuleCollider2D>();
                    CheckCollisions();
                }
            }
        }
        
        private bool IsGrounded()
        {
            return Physics2D.Raycast(CurrentUser.transform.position, Vector2.down, groundCheckDistance, groundLayer);
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
                    
                    AbilitySystemComponent asc = collider.gameObject.GetComponent<AbilitySystemComponent>();
                    if (asc == null) continue;
                    foreach (var effect in gameplayEffects)
                    {
                        asc.ApplyEffect(effect);
                    }
                    
                    //Apply mana regain effect to self.
                    if (GetAbilitySystemComponent() != null)
                    {
                        GetAbilitySystemComponent().ApplyEffect(onHitEffect);
                    }
                }
                
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
