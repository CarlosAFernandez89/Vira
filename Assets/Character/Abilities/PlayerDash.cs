using GameplayAbilitySystem;
using GameplayAbilitySystem.Abilities;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Character.Abilities
{
    [CreateAssetMenu(fileName = "Dash", menuName = "Abilities/Player/Dash")]
    public class PlayerDash : GameplayAbilityBase
    {
        [Header("Base Values")]
        [SerializeField] private float dashSpeed = 1.5f;
        [SerializeField] private AnimationCurve dashSpeedCurve;
        [SerializeField] private bool useInputDirection = false;
        [SerializeField] private InputActionReference movementAction;
        [SerializeField] private bool resetVelocityOnEnd = false;
        [SerializeField] public bool overrideDashTime = false;
        [SerializeField, Tooltip("Set the dash time if overrideDashTime is enabled.")] 
        public float newDashTime = 1.5f;
        
        private Vector2 _dashDirection = Vector2.zero;
        private float _dashDuration = 0f;

        [Header("Collision Settings")] 
        [SerializeField] private LayerMask collisionLayer;
        private ContactFilter2D _contactFilter;
        private RaycastHit2D[] _hit = new RaycastHit2D[10];

        private GameObject _playerObject;
        private Rigidbody2D _rigidbody2D;
        private Coroutine _dashCoroutine;

        public override void OnAbilityGranted(AbilitySystemComponent owningAbilitySystemComponent)
        {
            base.OnAbilityGranted(owningAbilitySystemComponent);
            
            //Don't end ability when the animation is done playing.
            endAbilityOnAnimationEnd = false;

            _playerObject = owningAbilitySystemComponent.gameObject;
            _rigidbody2D = _playerObject.GetComponent<Rigidbody2D>();
            
            _dashDuration = overrideDashTime ? newDashTime : abilityAnimation.length;
            AbilitySystemLogger.Log("Dash Duration: " + _dashDuration);

            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = collisionLayer,
                useLayerMask = true,
            };
        }

        protected override void ActivateAbility(GameObject user)
        {
            base.ActivateAbility(user);
            
            Dash();
        }

        private Vector2 GetDashDirection()
        {
            if (movementAction != null && movementAction.action != null && useInputDirection)
            {
                Vector2 inputDirection = movementAction.action.ReadValue<Vector2>();
                return inputDirection.sqrMagnitude > 0 ? inputDirection.normalized : Vector2.zero;
            }

            return new Vector2(Mathf.Sign(_playerObject.transform.right.x), 0f);
        }

        private void Dash()
        {
            _dashDirection = GetDashDirection();
            
            if (_dashCoroutine != null)
            {
                CoroutineRunner.Instance.StopRoutine(DashCoroutine());
            }

            _dashCoroutine = CoroutineRunner.Instance.StartRoutine(DashCoroutine());
        }
        
        private System.Collections.IEnumerator DashCoroutine()
        {
            float elapsedTime = 0f;

            while (elapsedTime < _dashDuration)
            {
                float normalizedTime = elapsedTime / _dashDuration;
                float speedMultiplier = dashSpeedCurve.Evaluate(normalizedTime);
                _rigidbody2D.linearVelocity = _dashDirection.normalized * (speedMultiplier * dashSpeed);

                if (CheckCollisions(_rigidbody2D.linearVelocity))
                {
                    if(resetVelocityOnEnd)
                        _rigidbody2D.linearVelocity = Vector2.zero;
            
                    EndAbility();
                }
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            if(resetVelocityOnEnd)
                _rigidbody2D.linearVelocity = Vector2.zero;
            
            EndAbility();
        }

        private bool CheckCollisions(Vector2 velocity)
        {
            // Cast the Rigidbody2D in the direction of velocity
            int hitCount = _rigidbody2D.Cast(velocity.normalized, _contactFilter, _hit, velocity.magnitude * Time.deltaTime);
            return hitCount > 0;
        }

        protected override void EndAbility()
        {
            base.EndAbility();
            _dashCoroutine = null;
        }

        protected override void HandleAnimationEvent(string eventName)
        {
            
        }
    }
}
