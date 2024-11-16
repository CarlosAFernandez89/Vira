using UnityEngine;
using UnityEngine.Serialization;

namespace Character
{       
    
    public enum MovementState
    {
        Grounded,
        Airborne,
        WaterSurface,
        Submerged,
        Climbing,
        Sliding,
        Crouched,
        Sprinting,
        Idle
    }
    
    [CreateAssetMenu(menuName = "PlayerMovementValues")]
    public class PlayerMovementValues : ScriptableObject
    {
        [Header("Walk")]
        [Range(1f, 100f)] public float maxWalkSpeed = 12.5f;
        [Range(0.25f, 50f)] public float groundAcceleration = 5f;
        [Range(0.25f, 50f)] public float groundDeceleration = 20f;
        [Range(0.25f, 50f)] public float airAcceleration = 5f;
        [Range(0.25f, 50f)] public float airDeceleration = 20f;
        
        [Header("Ground/Collision Checks")]
        public LayerMask groundLayer;
        public float groundCheckDistance = 0.02f;
        public float headCheckDistance = 0.02f;
        [Range(0f, 1f)] public float headWidth = 0.75f;
        
        [Header("Jump")]
        public float jumpHeight = 6.5f;
        [Range(1f, 1.1f)] public float jumpHeightCompensationFactor = 1.054f;
        public float timeUntilJumpApex = 0.35f;
        [Range(0.01f, 5f)] public float gravityOnReleaseMultiplier = 2f;
        public float maxFallSpeed = 26f;
        [Range(1,5)] public int numberOfJumpsAllowed = 2;

        [Header("Jump Cut")] [Range(0.02f, 0.3f)]
        public float timeForUpwardsCancel = 0.027f;
        
        [Header("Jump Apex")]
        [Range(0.5f, 1f)] public float apexThreshold = 0.97f;
        [Range(0.01f, 1f)] public float apexHangTime = 0.075f;

        [Header("Jump Buffer")]
        [Range(0f, 1f)] public float jumpBufferTime = 0.125f;
        
        [Header("Jump Coyote Time")]
        [Range(0f, 1f)] public float jumpCoyoteTime = 0.1f;
        
        [Header("Debug")]
        public bool bDebugShowIsGroundedBox;
        public bool bDebugShowIsCeilingBox;

        [Header("Jump Visualization Tool")]
        public bool bShowWalkJumpArc = false;
        public bool bShowRunJumpArc = false;
        public bool bStopOnCollision = true;
        public bool bDrawRight = true;
        [Range(5, 100)] public int arcResolution = 20;
        [Range(0, 500)] public float visualizationSteps = 90;
        
        public float Gravity { get; private set; }
        
        public float InitialJumpVelocity { get; private set; }
        
        public float AdjustedJumpHeight { get; private set; }
        
        public MovementState currentMovementState = MovementState.Idle;


        private void OnValidate()
        {
            CalculateValues();
        }

        private void OnEnable()
        {
            CalculateValues();
        }

        private void CalculateValues()
        {
            AdjustedJumpHeight = jumpHeight * jumpHeightCompensationFactor;
            Gravity = -(2f * AdjustedJumpHeight) / Mathf.Pow(timeUntilJumpApex, 2f);
            InitialJumpVelocity = Mathf.Abs(Gravity) * timeUntilJumpApex;
        }
    }
}
