using System;
using System.Collections;
using GameplayAbilitySystem;
using GameplayAbilitySystem.Abilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Abilities
{
    [CreateAssetMenu(fileName = "GA_PlayerJump", menuName = "Abilities/Player/Jump")]
    public class PlayerJump : GameplayAbilityBase
    {
        [Header("Jump Values")] 
        [SerializeField] [Range(0.02f, 0.1f)] private float groundCheckDistance;
        [SerializeField] [Range(0.02f, 0.1f)] private float headCheckDistance;
        [SerializeField] [Range(0f, 1f)] public float headWidth = 0.75f;
        [SerializeField] LayerMask groundLayer;


        [Header("References")] 
        private Collider2D _feetCollider;
        private Collider2D _bodyCollider;
        private PlayerMovementValues _playerMovementValues;
        
        [Header("Collision Checks")]
        private RaycastHit2D _groundHit;
        private RaycastHit2D _headHit;
        private bool _bumpedHead;
        
        [Header("Debug")]
        public bool bDebugShowIsGroundedBox;
        public bool bDebugShowIsCeilingBox;
        
        
        delegate void OnPlayerLanded();
        OnPlayerLanded _onPlayerLanded;

        private IEnumerator _collisionCheckCoroutine;
        

        public override void OnAbilityGranted(AbilitySystemComponent owningAbilitySystemComponent)
        {
            base.OnAbilityGranted(owningAbilitySystemComponent);
            
            GameObject player = owningAbilitySystemComponent.gameObject;
            _playerMovementValues = player.GetComponent<PlayerMovement>().movementValues;
            GameObject body = player.transform.Find("BodyCollider").gameObject;
            _bodyCollider = body.GetComponent<Collider2D>();
            GameObject feet = player.transform.Find("FeetCollider").gameObject;
            _feetCollider = feet.GetComponent<Collider2D>();
            
        }

        protected override void ActivateAbility(GameObject user)
        {
            base.ActivateAbility(user);

            _onPlayerLanded += EndAbility;
            
            // Must delay the collision checks to allow for the player to get off the ground. If it 
            // doesn't get off the ground by they delay count it must have already landed so it will just
            // end the ability.
            _collisionCheckCoroutine = JumpCollisionChecksOnTick();
            CoroutineRunner.Instance.StartRoutineWithDelay(_collisionCheckCoroutine, 0.1f);

        }

        protected override void EndAbility()
        {
            // Ensure to stop the correct coroutine by using the stored IEnumerator reference
            if (_collisionCheckCoroutine != null)
            {
                CoroutineRunner.Instance.StopRoutine(_collisionCheckCoroutine);
                _collisionCheckCoroutine = null;
            }
            
            _onPlayerLanded -= EndAbility;
            
            base.EndAbility();
        }

        protected override void ApplyCost(GameObject user)
        {
            base.ApplyCost(user);
        }

        protected override void HandleAnimationEvent(string eventName)
        {
            
        }

        private IEnumerator JumpCollisionChecksOnTick()
        {
            while (true)
            {
                CollisionChecks();
                yield return null; // Wait for the next frame
            }
        }
        
        #region Collision Checks

        private void CollisionChecks()
        {
            //AbilitySystemLogger.Log("Running Jump collision checks");
            GroundCheck();
            CeilingCheck();
        }

        private void GroundCheck()
        {
            if (_feetCollider == null || _bodyCollider == null) return;
            
            Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _feetCollider.bounds.min.y);
            Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x, groundCheckDistance);

            _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down,
                groundCheckDistance, groundLayer);
            if (_groundHit.collider != null)
            {
                _playerMovementValues.currentMovementState = MovementState.Grounded;
                _bumpedHead = false;
                
                _onPlayerLanded?.Invoke();
                return;
            }
            
            _playerMovementValues.currentMovementState = MovementState.Airborne;
            
            #region Debug Visualization

            if (bDebugShowIsGroundedBox)
            {
                var rayColor = (_playerMovementValues.currentMovementState == MovementState.Grounded) ? Color.green : Color.red;
                
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * groundCheckDistance, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * groundCheckDistance, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - groundCheckDistance), Vector2.right * boxCastSize.x, rayColor);
            }

            #endregion

        }

        private void CeilingCheck()
        {
            if(_feetCollider == null || _bodyCollider == null) return;
            
            Vector2 boxCastOrigin = new Vector2(_feetCollider.bounds.center.x, _bodyCollider.bounds.max.y);
            Vector2 boxCastSize = new Vector2(_feetCollider.bounds.size.x * headWidth, headCheckDistance);

            _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up,
                headCheckDistance, groundLayer);
            
            if (_headHit.collider != null)
            {
                _bumpedHead = true;
                return;
            }
            
            #region Debug Visualization

            if (bDebugShowIsCeilingBox)
            {
                var rayColor = (_playerMovementValues.currentMovementState == MovementState.Grounded) ? Color.green : Color.red;
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * headCheckDistance, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * headCheckDistance, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y - groundCheckDistance), Vector2.right * (boxCastSize.x * headWidth), rayColor);
            }
            #endregion
        }
        
        #endregion
    }
}
