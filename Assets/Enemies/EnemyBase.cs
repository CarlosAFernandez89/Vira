using System;
using GameplayAbilitySystem;
using UnityEngine;

namespace Enemies
{
    public class EnemyBase : MonoBehaviour, IAbilitySystemComponent
    {
        private bool _isFacingRight = false;
        private Rigidbody2D _rigidbody2D;
        
        public AbilitySystemComponent GetAbilitySystemComponent()
        {
            return GetComponent(typeof(AbilitySystemComponent)) as AbilitySystemComponent;
        }

        private void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            TurnCheck();
        }

        private void TurnCheck()
        {
            // Check the velocity on the x-axis to see if moving left or right
            if (_rigidbody2D.linearVelocity.x > 0f && !_isFacingRight)  // Moving right, but facing left
            {
                Turn(true); // Turn to face right
            }
            else if (_rigidbody2D.linearVelocity.x < 0f && _isFacingRight) // Moving left, but facing right
            {
                Turn(false); // Turn to face left
            }
        }
        
        private void Turn(bool bTurnRight)
        {
            if (bTurnRight)
            {
                _isFacingRight = true;
                transform.Rotate(0f, 180f, 0f);
            }
            else
            {
                _isFacingRight = false;
                transform.Rotate(0f, -180f, 0f);
            }
        }
    }
}
