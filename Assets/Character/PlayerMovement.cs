using BandoWare.GameplayTags;
using Character.Camera;
using GameplayAbilitySystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Character
{
    public class PlayerMovement : MonoBehaviour
    {

        #region Variables
        private Rigidbody2D _rigidbody2D;
        
        public Animator animator;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsJumping = Animator.StringToHash("isJumping");


        private Vector2 _moveDirection;
        
        [Header("References")]
        [SerializeField] public PlayerMovementValues movementValues;
        [SerializeField] private Collider2D feetCollider;
        [SerializeField] private Collider2D bodyCollider;
        [SerializeField] private InputActionReference moveAction;
        [SerializeField] private InputActionReference jumpAction;
        
        //Movement
        private Vector2 _moveVelocity;
        public bool isFacingRight;
        
        
        //Collision check
        private RaycastHit2D _groundHit;
        private RaycastHit2D _headHit;
        private bool _bumpedHead;
        
        //Jumping
        private float VerticalVelocity { get; set; }
        private bool _isJumping;
        private bool _isFastFalling;
        private bool _isFalling;
        private float _fastFallTime;
        private float _fastFallReleaseSpeed;
        private int _numberOfJumpsUsed;

        private float _apexPoint;
        private float _timePastApexThreshold;
        private bool _isPastApexThreshold;

        private float _jumpBufferTimer;
        private bool _jumpReleasedDuringBuffer;

        private float _coyoteTimer;
        
        private CameraFollow _cameraFollow;
        
        private float _fallSpeedYDampingChangeThreshold;
        
        private AbilitySystemComponent _abilitySystemComponent;

        #endregion
        
        #region UnityFunctionCalls
        
        private void OnEnable()
        {
            jumpAction.action.started += JumpStart;
            jumpAction.action.canceled += JumpEnd;
        }

        private void OnDisable()
        {
            jumpAction.action.started -= JumpStart;
            jumpAction.action.canceled -= JumpEnd;
        }

        private void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _abilitySystemComponent = GetComponent<AbilitySystemComponent>();
            var fCam = GameObject.FindWithTag("FollowCamera");
            _cameraFollow = fCam.GetComponent<CameraFollow>();
            isFacingRight = true;
            _fallSpeedYDampingChangeThreshold = CameraManager.instance.fallSpeedYDampingChangeThreshold;
        }

        private void OnDrawGizmos()
        {
            if (movementValues.bShowWalkJumpArc)
            {
                DrawJumpArc();
            }
        }

        private void Update()
        {
            CountTimers();
            JumpChecks();
            CameraYDamping();

        }

        private void CameraYDamping()
        {
            if (movementValues.currentMovementState == MovementState.Grounded)
            {
                return;
            }
            
            if (_rigidbody2D.linearVelocity.y < _fallSpeedYDampingChangeThreshold &&
                !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
            {
                CameraManager.instance.LerpYDamping(true);
            }

            if (_rigidbody2D.linearVelocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping &&
                CameraManager.instance.LerpedFromPlayerFalling)
            {
                CameraManager.instance.LerpedFromPlayerFalling = false;
                CameraManager.instance.LerpYDamping(false);
            }
        }

        private void FixedUpdate()
        {
            CollisionChecks();
            Move();
        }

        #endregion
        
        #region Movement

        private bool IsMovementBlocked()
        {
            foreach (var appliedTags in _abilitySystemComponent.AppliedTags.GetTags())
            {
                if (appliedTags.Equals("MovementBlocked"))
                {
                    return true;
                }
            }
            return false;
        }
        
        private void Move()
        {
            if (IsMovementBlocked()) return;
            
            switch (movementValues.currentMovementState)
            {
                case MovementState.Grounded:
                {
                    Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
                
                    if (moveInput != Vector2.zero)
                    {
                        TurnCheck();
                    
                        Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * movementValues.maxWalkSpeed;
                    
                        _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, movementValues.groundAcceleration * Time.deltaTime);
                    }
                    else
                    {
                        _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, movementValues.groundDeceleration * Time.deltaTime);
                    }
                    
                    _rigidbody2D.linearVelocity = new Vector2(_moveVelocity.x, _rigidbody2D.linearVelocity.y);
                    
                    break;
                }
                case MovementState.Airborne:
                {
                    Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
                
                    if (moveInput != Vector2.zero)
                    {
                        TurnCheck();
                    
                        Vector2 targetVelocity = new Vector2(moveInput.x, 0f) * movementValues.maxWalkSpeed;
                    
                        _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, movementValues.airAcceleration * Time.deltaTime);
                    }
                    else
                    {
                        _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, movementValues.airDeceleration * Time.deltaTime);
                    }
                    
                    _rigidbody2D.linearVelocity = new Vector2(_moveVelocity.x, _rigidbody2D.linearVelocity.y);
                    
                    break;
                }
                default: break;
            }
            
            
            // Normal gravity while falling
            if (movementValues.currentMovementState != MovementState.Grounded && !_isJumping)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                }
                
                VerticalVelocity += movementValues.Gravity * Time.fixedDeltaTime;
            }

            // Clamp fall speed
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -movementValues.maxFallSpeed, 50f);
            
            _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocity.x, VerticalVelocity);
            //_rigidbody2D.linearVelocity = new Vector2(_moveVelocity.x, _rigidbody2D.linearVelocity.y);
            
            animator.SetFloat(Speed, _moveVelocity.magnitude);
        }

        private void TurnCheck()
        {
            switch (isFacingRight)
            {
                case true when moveAction.action.ReadValue<Vector2>().x < 0f:
                    Turn(false);
                    break;
                case false when moveAction.action.ReadValue<Vector2>().x > 0f:
                    Turn(true);
                    break;
            }
        }

        private void Turn(bool bTurnRight)
        {
            if (bTurnRight)
            {
                isFacingRight = true;
                transform.Rotate(0f, 180f, 0f);
            }
            else
            {
                isFacingRight = false;
                transform.Rotate(0f, -180f, 0f);
            }

            _cameraFollow.CallTurn();
        }
        
        #endregion

        #region Jump
        
        private void InitiateJump(int numberOfJumpsUsed)
        {
            if (!_isJumping)
            {
                _isJumping = true;
                animator.SetBool(IsJumping, true);
            }

            _jumpBufferTimer = 0f;
            _numberOfJumpsUsed += numberOfJumpsUsed;
            VerticalVelocity = movementValues.InitialJumpVelocity;
        }

        private void JumpChecks()
        {
            if (_isJumping) // _abilitySystemComponent.GameplayTags.HasTag("Ability.Jump")
            {
                if (_bumpedHead)
                {
                    _isFastFalling = true;
                }

                if (VerticalVelocity >= 0f)
                {
                    _apexPoint = Mathf.InverseLerp(movementValues.InitialJumpVelocity, 0f, VerticalVelocity);

                    if (_apexPoint > movementValues.apexThreshold)
                    {
                        if (!_isPastApexThreshold)
                        {
                            _isPastApexThreshold = true;
                            _timePastApexThreshold = 0f;
                        }

                        if (_isPastApexThreshold)
                        {
                            _timePastApexThreshold += Time.fixedDeltaTime;
                            if (_timePastApexThreshold < movementValues.apexHangTime)
                            {
                                VerticalVelocity = 0f;
                            }
                            else
                            {
                                VerticalVelocity = -0.01f;
                            }
                        }
                    }
                    else // Gravity on ascending but not past apex threshold
                    {
                        VerticalVelocity += movementValues.Gravity * Time.deltaTime;
                        if (_isPastApexThreshold)
                        {
                            _isPastApexThreshold = false;
                        }
                    }
                }
                else if (!_isFastFalling)
                {
                    VerticalVelocity += movementValues.Gravity * movementValues.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (VerticalVelocity < 0f)
                {
                    if (!_isFastFalling)
                    {
                        _isFastFalling = true;
                    }
                }
            }

            // Jump Cut
            if (_isFastFalling)
            {
                if (_fastFallTime >= movementValues.timeForUpwardsCancel)
                {
                    VerticalVelocity += movementValues.Gravity * movementValues.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (_fastFallTime < movementValues.timeForUpwardsCancel)
                {
                    VerticalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f,
                        (_fastFallTime / movementValues.timeForUpwardsCancel));
                }
                
                _fastFallTime += Time.fixedDeltaTime;
            }
            
            
            // Jump with JumpBuffering and Coyote Time
            if (_jumpBufferTimer > 0f && !_isJumping && ((movementValues.currentMovementState == MovementState.Grounded) || _coyoteTimer > 0f))
            {
                InitiateJump(1);

                if (_jumpReleasedDuringBuffer)
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            } // Double Jump
            else if (_jumpBufferTimer > 0f && _isJumping &&
                     _numberOfJumpsUsed < movementValues.numberOfJumpsAllowed)
            {
                _isFastFalling = false;
                InitiateJump(1);
            }
            // Air jump after coyote time elapse
            else if (_jumpBufferTimer > 0f && _isFalling && _numberOfJumpsUsed < movementValues.numberOfJumpsAllowed - 1)
            {   
                InitiateJump(2);
                _isFastFalling = false;
            }

            // Landed
            if ((_isJumping || _isFalling) && (movementValues.currentMovementState == MovementState.Grounded) && VerticalVelocity <= 0f)
            {
                _isJumping = false;
                _isFalling = false;
                _isFastFalling = false;
                _fastFallTime = 0f;
                _isPastApexThreshold = false;
                _numberOfJumpsUsed = 0;
                
                VerticalVelocity = Physics2D.gravity.y;
                animator.SetBool(IsJumping, false);
            }
            
        }

        public void JumpStart(InputAction.CallbackContext context)
        {
            _jumpBufferTimer = movementValues.jumpBufferTime;
            _jumpReleasedDuringBuffer = false;
        }

        public void JumpEnd(InputAction.CallbackContext context)
        {
            if (_jumpBufferTimer > 0f)
            {
                _jumpReleasedDuringBuffer = true;
            }

            if (_isJumping && VerticalVelocity > 0f)
            {
                if (_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = movementValues.timeForUpwardsCancel;
                    VerticalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticalVelocity;
                }
            }
        }
        #endregion

        #region Collision Checks

        private void CollisionChecks()
        {
            GroundCheck();
            CeilingCheck();
        }

        private void GroundCheck()
        {
            Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
            Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x, movementValues.groundCheckDistance);

            _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down,
                movementValues.groundCheckDistance, movementValues.groundLayer);
            if (_groundHit.collider != null)
            {
                movementValues.currentMovementState = MovementState.Grounded;
                _bumpedHead = false;
                return;
            }
            
            movementValues.currentMovementState = MovementState.Airborne;
            
            #region Debug Visualization

            if (movementValues.bDebugShowIsGroundedBox)
            {
                var rayColor = (movementValues.currentMovementState == MovementState.Grounded) ? Color.green : Color.red;
                
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * movementValues.groundCheckDistance, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * movementValues.groundCheckDistance, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - movementValues.groundCheckDistance), Vector2.right * boxCastSize.x, rayColor);
            }

            #endregion

        }

        private void CeilingCheck()
        {
            Vector2 boxCastOrigin = new Vector2(feetCollider.bounds.center.x, bodyCollider.bounds.max.y);
            Vector2 boxCastSize = new Vector2(feetCollider.bounds.size.x * movementValues.headWidth, movementValues.headCheckDistance);

            _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up,
                movementValues.headCheckDistance, movementValues.groundLayer);
            
            if (_headHit.collider != null)
            {
                _bumpedHead = true;
                return;
            }
            
            #region Debug Visualization

            if (movementValues.bDebugShowIsCeilingBox)
            {
                float headWidth = movementValues.headWidth;
                var rayColor = (movementValues.currentMovementState == MovementState.Grounded) ? Color.green : Color.red;
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * movementValues.headCheckDistance, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * movementValues.headCheckDistance, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y - movementValues.groundCheckDistance), Vector2.right * (boxCastSize.x * headWidth), rayColor);
            }
            #endregion
        }

        private void DrawJumpArc()
        {
            Vector2 startPosition = new Vector2(feetCollider.bounds.center.x, feetCollider.bounds.min.y);
            Vector2 previousPosition = startPosition;
            float speed = 0f;
            if (movementValues.bDrawRight)
            {
                speed = movementValues.maxWalkSpeed;
            }else { speed = -movementValues.maxWalkSpeed; }
            
            Vector2 velocity = new Vector2(speed, movementValues.InitialJumpVelocity);
            
            Gizmos.color = Color.yellow;

            float timeStep = 2 * movementValues.timeUntilJumpApex / movementValues.arcResolution;

            for (int i = 0; i < movementValues.visualizationSteps; i++)
            {
                float simulationTime = i * timeStep;
                Vector2 displacement;
                Vector2 drawPoint;

                if (simulationTime < movementValues.timeUntilJumpApex)
                {
                    displacement = velocity * simulationTime +
                                   0.5f * new Vector2(0, movementValues.Gravity) * simulationTime * simulationTime;
                }
                else if (simulationTime < movementValues.timeUntilJumpApex + movementValues.apexHangTime)
                {
                    float apexTime = simulationTime - movementValues.timeUntilJumpApex;
                    displacement = velocity * movementValues.timeUntilJumpApex + 0.5f * new Vector2(0, movementValues.Gravity) * movementValues.timeUntilJumpApex * movementValues.timeUntilJumpApex;
                    displacement += new Vector2(speed, 0) * apexTime;
                }
                else
                {
                    float descendTime = simulationTime - (movementValues.timeUntilJumpApex + movementValues.apexHangTime);
                    displacement = velocity * movementValues.timeUntilJumpApex + 0.5f * new Vector2(0, movementValues.Gravity) * movementValues.timeUntilJumpApex * movementValues.timeUntilJumpApex;
                    displacement += new Vector2(speed, 0) * movementValues.apexHangTime;
                    displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, movementValues.Gravity) * descendTime * descendTime;
                }
                
                drawPoint = startPosition + displacement;

                if (movementValues.bStopOnCollision)
                {
                    RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), movementValues.groundLayer);
                    if (hit.collider != null)
                    {
                        Gizmos.DrawLine(previousPosition, hit.point);
                        break;
                    }
                }
                
                Gizmos.DrawLine(previousPosition, drawPoint);
                previousPosition = drawPoint;
            }
        }
        #endregion
        
        #region Timers

        private void CountTimers()
        {
            _jumpBufferTimer -= Time.deltaTime;
            
            if (movementValues.currentMovementState != MovementState.Grounded)
            {
                _coyoteTimer -= Time.deltaTime;
            }
            else
            {
                _coyoteTimer = movementValues.jumpCoyoteTime;
            }
        }
        #endregion

    }
}
